using Bank.Domain.Events;
using Bank.Domain.Exceptions;

namespace Bank.Domain.Entities
{
    public class BankAccount
    {
        public Guid Id { get; private set; }

        public decimal Balance { get; private set; }

        public long Version { get; private set; }

        private BankAccount()
        {
            // Required by EF Core
        }

        public BankAccount(Guid id, decimal openingBalance)
        {
            if (id == Guid.Empty)
                throw new ArgumentException(
                    "Account ID cannot be empty.");

            if (openingBalance < 0)
                throw new ArgumentException(
                    "Opening balance cannot be negative.");

            Id = id;
            Balance = openingBalance;
        }

        public WithdrawalCompletedEvent Withdraw(decimal amount, DateTimeOffset occurredAt)
        {
            if (amount <= 0)
                throw new InvalidWithdrawalException(
                    "Withdrawal amount must be greater than zero.");

            if (Balance < amount)
                throw new InsufficientFundsException();

            // Apply the business operation.
            Balance -= amount;

            // Increment the optimistic concurrency version.
            Version++;

            return new WithdrawalCompletedEvent(
                Guid.NewGuid(),
                Id,
                amount,
                Balance,
                occurredAt
            );
        }
    }
}
