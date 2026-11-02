namespace Bank.Application.Exceptions
{
    public sealed class IdempotencyKeyReuseException
        : Exception
    {
        public IdempotencyKeyReuseException()
            : base(
                "Idempotency key has already been used " +
                "with a different request.")
        {
        }
    }
}
