using Bank.Application.Interfaces;
using Bank.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bank.Infrastructure.Persistence.Repositories
{
    public sealed class AccountRepository(
        BankingDbContext context)
        : IAccountRepository
    {
        public async Task<BankAccount?> GetByIdAsync(
            Guid accountId,
            CancellationToken cancellationToken)
        {
            return await context.Accounts
                .FirstOrDefaultAsync(
                    x => x.Id == accountId,
                    cancellationToken);
        }
    }
}
