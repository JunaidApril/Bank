namespace Bank.Application.Exceptions
{
    public sealed class AccountNotFoundException : Exception
    {
        public Guid AccountId { get; }

        public AccountNotFoundException(Guid accountId)
            : base($"Bank account with ID '{accountId}' was not found.")
        {
            AccountId = accountId;
        }
    }
}