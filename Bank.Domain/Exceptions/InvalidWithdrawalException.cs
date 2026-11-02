namespace Bank.Domain.Exceptions
{
    public sealed class InvalidWithdrawalException : Exception
    {
        public InvalidWithdrawalException(string message)
            : base(message)
        {
        }
    }
}
