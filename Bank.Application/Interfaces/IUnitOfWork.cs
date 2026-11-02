namespace Bank.Application.Interfaces
{
    public interface IUnitOfWork
    {
        Task<IUnitOfWorkTransaction> BeginTransactionAsync(
            CancellationToken cancellationToken);
        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}