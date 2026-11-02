namespace Bank.API.Contracts.Responses
{
    public sealed record WithdrawResponse(
        Guid WithdrawalId,
        Guid AccountId,
        decimal Amount,
        decimal RemainingBalance
    );
}
