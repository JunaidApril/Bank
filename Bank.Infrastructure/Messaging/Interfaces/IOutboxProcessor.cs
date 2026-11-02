namespace Bank.Infrastructure.Messaging.Interfaces
{
    public interface IOutboxProcessor
    {
        Task ProcessBatchAsync(CancellationToken cancellationToken);
    }
}
