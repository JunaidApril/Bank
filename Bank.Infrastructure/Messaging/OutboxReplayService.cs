using Bank.Application.Interfaces;

namespace Bank.Infrastructure.Messaging
{
    public sealed class OutboxReplayService(
        IOutboxRepository repository,
        TimeProvider timeProvider)
        : IOutboxReplayService
    {
        public Task<bool> ReplayAsync(
            Guid eventId,
            CancellationToken cancellationToken)
        {
            return repository.ReplayDeadLetterAsync(
                eventId,
                timeProvider.GetUtcNow(),
                cancellationToken);
        }
    }
}