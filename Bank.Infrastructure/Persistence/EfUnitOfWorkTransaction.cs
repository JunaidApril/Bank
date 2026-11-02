using Bank.Application.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Bank.Infrastructure.Persistence
{
    public sealed class EfUnitOfWorkTransaction(
        IDbContextTransaction transaction)
        : IUnitOfWorkTransaction
    {
        public Task CommitAsync(
            CancellationToken cancellationToken)
        {
            return transaction.CommitAsync(cancellationToken);
        }

        public Task RollbackAsync(
            CancellationToken cancellationToken)
        {
            return transaction.RollbackAsync(cancellationToken);
        }

        public ValueTask DisposeAsync()
        {
            return transaction.DisposeAsync();
        }
    }
}
