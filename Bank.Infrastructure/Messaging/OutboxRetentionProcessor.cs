using Bank.Infrastructure.Messaging.Configurations;
using Bank.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bank.Infrastructure.Messaging
{
    public sealed class OutboxRetentionProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxRetentionProcessor> logger,
    IOptions<OutboxOptions> options,
    TimeProvider timeProvider)
    : BackgroundService
    {
        private static readonly TimeSpan CleanupInterval =
            TimeSpan.FromHours(24);

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();

                    var context = scope.ServiceProvider
                        .GetRequiredService<BankingDbContext>();

                    var settings = options.Value;

                    var now = timeProvider.GetUtcNow();

                    var publishedCutoff = now.AddDays(-settings.RetentionDays);

                    var deadLetterCutoff = now.AddDays(-settings.DeadLetterRetentionDays);

                    // Delete old successfully published messages.
                    var publishedDeleted =
                        await context.OutboxMessages
                            .Where(x =>
                                x.PublishedAt != null &&
                                x.PublishedAt < publishedCutoff)
                            .ExecuteDeleteAsync(stoppingToken);

                    // Delete old dead-lettered messages only
                    // after the approved retention period.
                    var deadLetterDeleted =
                        await context.OutboxMessages
                            .Where(x =>
                                x.DeadLetteredAt != null &&
                                x.DeadLetteredAt < deadLetterCutoff)
                            .ExecuteDeleteAsync(stoppingToken);

                    logger.LogInformation(
                        "Outbox cleanup completed. " +
                        "Published: {PublishedDeleted}. " +
                        "DeadLettered: {DeadLetterDeleted}.",
                        publishedDeleted,
                        deadLetterDeleted);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    logger.LogError(
                        exception,
                        "Outbox retention cleanup failed.");
                }

                try
                {
                    await Task.Delay(
                        CleanupInterval,
                        stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }
}
