namespace Bank.API.Contracts.Requests
{
    public sealed record WithdrawRequest(
        decimal Amount
    );
}
