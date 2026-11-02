using Bank.Domain.Entities;

namespace Bank.Application.Interfaces
{
    public interface IWithdrawalRepository
    {
        Task AddAsync(
            WithdrawalTransaction withdrawal,
            CancellationToken cancellationToken);
    }
}
