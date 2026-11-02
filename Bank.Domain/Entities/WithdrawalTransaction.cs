using Bank.Domain.Enums;

namespace Bank.Domain.Entities
{
    public sealed class WithdrawalTransaction
    {
        public Guid Id { get; private set; }

        public Guid AccountId { get; private set; }

        public decimal Amount { get; private set; }

        public decimal BalanceBefore { get; private set; }

        public decimal BalanceAfter { get; private set; }

        public WithdrawalStatus Status { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }

        public string? CorrelationId { get; private set; }

        private WithdrawalTransaction()
        {
            // Required by EF Core.
        }

        private WithdrawalTransaction(
            Guid id,
            Guid accountId,
            decimal amount,
            decimal balanceBefore,
            decimal balanceAfter,
            DateTimeOffset createdAt,
            string? correlationId)
        {
            Id = id;
            AccountId = accountId;
            Amount = amount;
            BalanceBefore = balanceBefore;
            BalanceAfter = balanceAfter;
            CreatedAt = createdAt;
            CorrelationId = correlationId;
            Status = WithdrawalStatus.Completed;
        }

        public static WithdrawalTransaction Create(
            Guid id,
            Guid accountId,
            decimal amount,
            decimal balanceBefore,
            decimal balanceAfter,
            DateTimeOffset createdAt,
            string? correlationId)
        {
            if (id == Guid.Empty)
                throw new ArgumentException(
                    "Withdrawal ID is required.");

            if (accountId == Guid.Empty)
                throw new ArgumentException(
                    "Account ID is required.");

            if (amount <= 0)
                throw new ArgumentException(
                    "Withdrawal amount must be greater than zero.");

            if (balanceBefore < 0 || balanceAfter < 0)
                throw new ArgumentException(
                    "Account balances cannot be negative.");

            if (balanceBefore - amount != balanceAfter)
                throw new ArgumentException(
                    "Withdrawal balance calculation is invalid.");

            return new WithdrawalTransaction(
                id,
                accountId,
                amount,
                balanceBefore,
                balanceAfter,
                createdAt,
                correlationId);
        }
    }

}
