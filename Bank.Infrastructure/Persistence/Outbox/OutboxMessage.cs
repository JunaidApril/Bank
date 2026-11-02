namespace Bank.Infrastructure.Persistence.Outbox
{
    public sealed class OutboxMessage
    {
        public Guid Id { get; private set; }

        public string EventType { get; private set; } = null!;

        public string Payload { get; private set; } = null!;

        public DateTimeOffset OccurredAt { get; private set; }

        public DateTimeOffset? PublishedAt { get; private set; }

        public int Attempts { get; private set; }

        public DateTimeOffset? NextAttemptAt { get; private set; }

        public DateTimeOffset? LockedUntil { get; private set; }

        public Guid? LockId { get; private set; }

        public DateTimeOffset? DeadLetteredAt { get; private set; }

        public string? LastError { get; private set; }

        public string? CorrelationId { get; private set; }

        private OutboxMessage()
        {
        }

        public static OutboxMessage Create(
            Guid id,
            string eventType,
            string payload,
            DateTimeOffset occurredAt,
            string? correlationId = null)
        {
            if (id == Guid.Empty)
                throw new ArgumentException(
                    "Event ID cannot be empty.");

            ArgumentException.ThrowIfNullOrWhiteSpace(
                eventType);

            ArgumentException.ThrowIfNullOrWhiteSpace(
                payload);

            return new OutboxMessage
            {
                Id = id,
                EventType = eventType,
                Payload = payload,
                OccurredAt = occurredAt,
                CorrelationId = correlationId
            };
        }

        public void Claim(
            Guid lockId,
            DateTimeOffset lockedUntil)
        {
            if (lockId == Guid.Empty)
                throw new ArgumentException(
                    "Lock ID cannot be empty.");

            if (PublishedAt.HasValue ||
                DeadLetteredAt.HasValue)
            {
                throw new InvalidOperationException(
                    "Completed messages cannot be claimed.");
            }

            LockId = lockId;
            LockedUntil = lockedUntil;
        }

        public void MarkPublished(
            DateTimeOffset publishedAt)
        {
            if (PublishedAt.HasValue)
                return;

            PublishedAt = publishedAt;

            ClearLock();
        }

        public void RecordFailure(
            string error,
            DateTimeOffset nextAttemptAt,
            DateTimeOffset failedAt,
            int maximumAttempts)
        {
            if (maximumAttempts <= 0)
                throw new ArgumentOutOfRangeException(
                    nameof(maximumAttempts));

            Attempts++;

            LastError = string.IsNullOrWhiteSpace(error)
                ? "Unknown publishing error."
                : error.Length > 2000
                    ? error[..2000]
                    : error;

            ClearLock();

            if (Attempts >= maximumAttempts)
            {
                DeadLetteredAt = failedAt;
                NextAttemptAt = null;
                return;
            }

            NextAttemptAt = nextAttemptAt;
        }

        public void RequeueDeadLettered(DateTimeOffset requeuedAt)
        {
            if (!DeadLetteredAt.HasValue)
            {
                throw new InvalidOperationException(
                    "Only dead-lettered messages can be replayed.");
            }

            DeadLetteredAt = null;

            Attempts = 0;

            NextAttemptAt = requeuedAt;

            LastError = null;

            ClearLock();
        }

        public void EnsureLeaseOwnership(Guid lockId)
        {
            if (LockId != lockId)
            {
                throw new InvalidOperationException(
                    "Worker does not own this outbox message.");
            }
        }

        private void ClearLock()
        {
            LockId = null;
            LockedUntil = null;
        }
    }
}
