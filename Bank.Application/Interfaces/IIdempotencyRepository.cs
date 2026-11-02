using Bank.Application.Features.Withdrawals.DTOs;

namespace Bank.Application.Interfaces
{
    public interface IIdempotencyRepository
    {
        Task<StoredIdempotencyResult?> GetAsync(
            Guid accountId,
            string idempotencyKey,
            CancellationToken cancellationToken);

        Task AddAsync(
            Guid accountId,
            string idempotencyKey,
            string requestHash,
            Guid withdrawalId,
            DateTimeOffset createdAt,
            CancellationToken cancellationToken);
    }
}
