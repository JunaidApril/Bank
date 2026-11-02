using Bank.Infrastructure.Messaging.Configurations;
using Bank.Infrastructure.Messaging.Interfaces;
using Bank.Infrastructure.Observability;
using Bank.Infrastructure.Persistence;
using Bank.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;



namespace Bank.Infrastructure.Messaging
{
    public sealed class OutboxBatchProcessor(
        BankingDbContext context,
        ISnsEventPublisher snsPublisher,
        ILogger<OutboxBatchProcessor> logger,
        OutboxMetrics metrics,
        IOptions<OutboxOptions> options,
        TimeProvider timeProvider)
        : IOutboxProcessor
    {
        public async Task ProcessBatchAsync(
            CancellationToken cancellationToken)
        {
            var settings = options.Value;

            var now = timeProvider.GetUtcNow();
            var lockId = Guid.NewGuid();

            var lockedUntil = now.AddMinutes(
                settings.LeaseDurationMinutes);

            // Atomically claim available messages.
            var claimed = await context.Database
                .ExecuteSqlInterpolatedAsync($"""
                    ;WITH PendingMessages AS
                    (
                        SELECT TOP ({settings.BatchSize}) *
                        FROM dbo.OutboxMessages WITH
                            (UPDLOCK, READPAST, ROWLOCK)
                        WHERE PublishedAt IS NULL
                          AND DeadLetteredAt IS NULL
                          AND (
                              NextAttemptAt IS NULL
                              OR NextAttemptAt <= {now}
                          )
                          AND (
                              LockedUntil IS NULL
                              OR LockedUntil < {now}
                          )
                        ORDER BY OccurredAt, Id
                    )
                    UPDATE PendingMessages
                    SET LockId = {lockId},
                        LockedUntil = {lockedUntil};
                    """,
                    cancellationToken);

            if (claimed == 0)
                return;

            // Load messages without tracking.
            var messages = await context.OutboxMessages
                .AsNoTracking()
                .Where(x => x.LockId == lockId)
                .OrderBy(x => x.OccurredAt)
                .ToListAsync(cancellationToken);

            logger.LogInformation(
                "Claimed {MessageCount} outbox messages.",
                messages.Count);

            foreach (var message in messages)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var stopwatch = Stopwatch.StartNew();

                try
                {
                    await snsPublisher.PublishAsync(
                        message,
                        cancellationToken);

                    var publishedAt = timeProvider.GetUtcNow();

                    // Update only if this worker still owns
                    // the message.
                    var updated = await context.OutboxMessages
                        .Where(x =>
                            x.Id == message.Id &&
                            x.LockId == lockId &&
                            x.PublishedAt == null &&
                            x.DeadLetteredAt == null)
                        .ExecuteUpdateAsync(
                            setters => setters
                                .SetProperty(
                                    x => x.PublishedAt,
                                    publishedAt)
                                .SetProperty(
                                    x => x.LockId,
                                    (Guid?)null)
                                .SetProperty(
                                    x => x.LockedUntil,
                                    (DateTimeOffset?)null),
                            cancellationToken);

                    if (updated == 1)
                    {
                        metrics.RecordPublished();

                        logger.LogInformation(
                            "Outbox message {MessageId} published.",
                            message.Id);
                    }
                    else
                    {
                        logger.LogWarning(
                            "Message {MessageId} was published, " +
                            "but this worker no longer owns its lease.",
                            message.Id);
                    }
                }
                catch (OperationCanceledException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    var attempt = message.Attempts + 1;

                    var failedAt = timeProvider.GetUtcNow();

                    var nextAttempt = CalculateNextAttempt(
                        attempt,
                        failedAt);

                    var lastError = exception.Message.Length > 2000
                        ? exception.Message[..2000]
                        : exception.Message;

                    var deadLettered =
                        attempt >= settings.MaximumAttempts;

                    var updated = await context.OutboxMessages
                        .Where(x =>
                            x.Id == message.Id &&
                            x.LockId == lockId &&
                            x.PublishedAt == null &&
                            x.DeadLetteredAt == null)
                        .ExecuteUpdateAsync(
                            setters => setters
                                .SetProperty(
                                    x => x.Attempts,
                                    attempt)
                                .SetProperty(
                                    x => x.LastError,
                                    lastError)
                                .SetProperty(
                                    x => x.NextAttemptAt,
                                    deadLettered
                                        ? null
                                        : nextAttempt)
                                .SetProperty(
                                    x => x.DeadLetteredAt,
                                    deadLettered
                                        ? failedAt
                                        : null)
                                .SetProperty(
                                    x => x.LockId,
                                    (Guid?)null)
                                .SetProperty(
                                    x => x.LockedUntil,
                                    (DateTimeOffset?)null),
                            cancellationToken);

                    if (updated == 1)
                    {
                        metrics.RecordFailure();

                        if (deadLettered)
                        {
                            metrics.RecordDeadLettered();

                            logger.LogError(
                                exception,
                                "Message {MessageId} moved to dead letter.",
                                message.Id);
                        }
                        else
                        {
                            logger.LogWarning(
                                exception,
                                "Message {MessageId} failed on attempt {Attempt}.",
                                message.Id,
                                attempt);
                        }
                    }
                    else
                    {
                        logger.LogWarning(
                            "Could not update failed message {MessageId}. " +
                            "Worker lease was lost.",
                            message.Id);
                    }
                }
                finally
                {
                    stopwatch.Stop();

                    metrics.RecordProcessingDuration(
                        stopwatch.Elapsed.TotalMilliseconds);
                }
            }

            // Update pending message measurement.
            var pendingCount = await context.OutboxMessages
                .CountAsync(
                    x => x.PublishedAt == null &&
                         x.DeadLetteredAt == null,
                    cancellationToken);

            metrics.SetPending(pendingCount);
        }

        private static DateTimeOffset CalculateNextAttempt(
            int attempt,
            DateTimeOffset failedAt)
        {
            const double baseDelaySeconds = 2;
            const double maximumDelaySeconds = 300;

            var exponentialDelay =
                baseDelaySeconds *
                Math.Pow(2, attempt - 1);

            var cappedDelay = Math.Min(
                exponentialDelay,
                maximumDelaySeconds);

            // Up to 20% random jitter.
            var jitter =
                Random.Shared.NextDouble() * 0.2;

            return failedAt.AddSeconds(
                cappedDelay * (1 + jitter));
        }
    }
}