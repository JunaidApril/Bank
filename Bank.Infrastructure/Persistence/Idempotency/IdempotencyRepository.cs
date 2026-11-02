using Bank.Application.Features.Withdrawals.DTOs;
using Bank.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bank.Infrastructure.Persistence.Idempotency
{
    public sealed class IdempotencyRepository(
        BankingDbContext context)
        : IIdempotencyRepository
    {
        public async Task<StoredIdempotencyResult?> GetAsync(
            Guid accountId,
            string idempotencyKey,
            CancellationToken cancellationToken)
        {
            var record = await context.IdempotencyRecords
                .FromSqlInterpolated($"""
            SELECT *
            FROM dbo.IdempotencyRecords
                WITH (UPDLOCK, HOLDLOCK)
            WHERE AccountId = {accountId}
              AND IdempotencyKey = {idempotencyKey}
            """)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (record is null)
                return null;

            var withdrawal = await context.WithdrawalTransactions
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == record.WithdrawalId,
                    cancellationToken);

            if (withdrawal is null)
            {
                throw new InvalidOperationException(
                    "Idempotency record references a missing withdrawal.");
            }

            var result = new WithdrawalResult(
                withdrawal.Id,
                withdrawal.AccountId,
                withdrawal.Amount,
                withdrawal.BalanceAfter);

            return new StoredIdempotencyResult(
                record.RequestHash,
                result);
        }

        public async Task AddAsync(
            Guid accountId,
            string idempotencyKey,
            string requestHash,
            Guid withdrawalId,
            DateTimeOffset createdAt,
            CancellationToken cancellationToken)
        {
            var record = IdempotencyRecord.Create(
                accountId,
                idempotencyKey,
                requestHash,
                withdrawalId,
                createdAt);

            await context.IdempotencyRecords.AddAsync(
                record,
                cancellationToken);
        }
    }

}
