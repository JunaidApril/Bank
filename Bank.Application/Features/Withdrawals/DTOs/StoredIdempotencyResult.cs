namespace Bank.Application.Features.Withdrawals.DTOs
{
    public sealed record StoredIdempotencyResult(
    string RequestHash,
    WithdrawalResult Result
);
}
