namespace Bank.Application.Features.Withdrawals.Commands
{
    public sealed record WithdrawMoneyCommand(
        Guid AccountId,
        decimal Amount,
        string IdempotencyKey
    );
}
