namespace Bank.Domain.Exceptions
{
    public sealed class InsufficientFundsException : Exception
    {
        public InsufficientFundsException()
            : base("Insufficient funds for this withdrawal.")
        {
        }
    }
}
