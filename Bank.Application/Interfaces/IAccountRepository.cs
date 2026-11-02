using Bank.Domain.Entities;

namespace Bank.Application.Interfaces
{
    public interface IAccountRepository
    {
        Task<BankAccount?> GetByIdAsync(
            Guid accountId,
            CancellationToken cancellationToken);
    }
}
