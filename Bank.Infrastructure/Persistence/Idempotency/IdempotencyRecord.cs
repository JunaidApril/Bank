namespace Bank.Infrastructure.Persistence.Idempotency
{
    public sealed class IdempotencyRecord
    {
        public Guid Id { get; private set; }

        public Guid AccountId { get; private set; }

        public string IdempotencyKey { get; private set; } = null!;

        public string RequestHash { get; private set; } = null!;

        public Guid WithdrawalId { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        private IdempotencyRecord()
        {
            // Required by EF Core.
        }

        public static IdempotencyRecord Create(
            Guid accountId,
            string idempotencyKey,
            string requestHash,
            Guid withdrawalId,
            DateTimeOffset createdAt)
        {
            return new IdempotencyRecord
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                IdempotencyKey = idempotencyKey,
                RequestHash = requestHash,
                WithdrawalId = withdrawalId,
                CreatedAt = createdAt
            };
        }
    }

}
