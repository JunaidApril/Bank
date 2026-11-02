namespace Bank.Domain.Enums
{
    public enum WithdrawalStatus
    {
        Completed = 1,
        InsufficientFunds = 2,
        AccountNotFound = 3,
        Failed = 4
    }
}
