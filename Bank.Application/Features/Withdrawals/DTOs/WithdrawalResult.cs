namespace Bank.Application.Features.Withdrawals.DTOs
{
    public sealed record WithdrawalResult(
        Guid WithdrawalId,
        Guid AccountId,
        decimal Amount,
        decimal RemainingBalance
    );
}
