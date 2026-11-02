namespace Bank.Application.Interfaces
{
    public interface IOutboxRepository
    {
        Task AddAsync(
            Guid eventId,
            string eventType,
            string payload,
            DateTimeOffset occurredAt,
            string correlationId,
            CancellationToken cancellationToken);

        Task<bool> ReplayDeadLetterAsync(
            Guid eventId,
            DateTimeOffset requeuedAt,
            CancellationToken cancellationToken);
    }
}
