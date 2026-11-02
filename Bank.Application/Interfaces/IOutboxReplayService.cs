namespace Bank.Application.Interfaces
{
    public interface IOutboxReplayService
    {
        Task<bool> ReplayAsync(
            Guid eventId,
            CancellationToken cancellationToken);
    }
}
