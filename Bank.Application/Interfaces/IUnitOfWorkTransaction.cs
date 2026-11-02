namespace Bank.Application.Interfaces
{
    public interface IUnitOfWorkTransaction : IAsyncDisposable
    {
        Task CommitAsync(
            CancellationToken cancellationToken);

        Task RollbackAsync(
            CancellationToken cancellationToken);
    }
}
