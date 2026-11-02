namespace Bank.Infrastructure.Messaging.Configurations
{
    public sealed class OutboxOptions
    {
        public const string SectionName = "Outbox";

        public int BatchSize { get; set; } = 5;

        public int MaximumAttempts { get; set; } = 5;

        public int LeaseDurationMinutes { get; set; } = 5;

        public int PollingIntervalSeconds { get; set; } = 2;

        public int RetentionDays { get; set; } = 30;

        public int DeadLetterRetentionDays { get; set; } = 90;

        public int PublishTimeoutSeconds { get; set; } = 30;

        public int LeaseSafetyBufferSeconds { get; set; } = 30;
    }
}
