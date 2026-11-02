using Bank.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bank.Infrastructure.Persistence.Outbox
{
    public sealed class OutboxRepository(
        BankingDbContext context)
        : IOutboxRepository
    {
        public async Task AddAsync(
            Guid eventId,
            string eventType,
            string payload,
            DateTimeOffset occurredAt,
            string correlationId,
            CancellationToken cancellationToken)
        {
            var message = OutboxMessage.Create(
                eventId,
                eventType,
                payload,
                occurredAt,
                correlationId);

            await context.OutboxMessages.AddAsync(
                message,
                cancellationToken);
        }

        public async Task<bool> ReplayDeadLetterAsync(
            Guid eventId,
            DateTimeOffset requeuedAt,
            CancellationToken cancellationToken)
        {
            var message = await context.OutboxMessages
                .FirstOrDefaultAsync(
                    x => x.Id == eventId &&
                         x.DeadLetteredAt != null,
                    cancellationToken);

            if (message is null)
                return false;

            message.RequeueDeadLettered(requeuedAt);

            await context.SaveChangesAsync(
                cancellationToken);

            return true;
        }
    }
}
