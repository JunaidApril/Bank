namespace Bank.Domain.Events
{
    public sealed record WithdrawalCompletedEvent(
        Guid EventId,
        Guid AccountId,
        decimal Amount,
        decimal RemainingBalance,
        DateTimeOffset OccurredAt
    );
}
