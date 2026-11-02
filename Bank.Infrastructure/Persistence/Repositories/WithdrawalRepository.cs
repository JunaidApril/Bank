using Bank.Application.Interfaces;
using Bank.Domain.Entities;

namespace Bank.Infrastructure.Persistence.Repositories
{
    public sealed class WithdrawalRepository(
        BankingDbContext context)
        : IWithdrawalRepository
    {
        public async Task AddAsync(
            WithdrawalTransaction withdrawal,
            CancellationToken cancellationToken)
        {
            await context.WithdrawalTransactions.AddAsync(
                withdrawal,
                cancellationToken);
        }
    }
}
