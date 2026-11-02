using Bank.Infrastructure.Persistence.Outbox;

namespace Bank.Infrastructure.Messaging.Interfaces
{
    public interface ISnsEventPublisher
    {
        Task PublishAsync(
            OutboxMessage message,
            CancellationToken cancellationToken);
    }
}
