using Bank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bank.Infrastructure.Persistence.Configurations
{
    public sealed class WithdrawalTransactionConfiguration
        : IEntityTypeConfiguration<WithdrawalTransaction>
    {

        public void Configure(EntityTypeBuilder<WithdrawalTransaction> builder)
        {
            builder.ToTable("WithdrawalTransactions", table =>
            {
                table.HasCheckConstraint(
                    "CK_WithdrawalTransactions_Amount_Positive",
                    "[Amount] > 0");

                table.HasCheckConstraint(
                    "CK_WithdrawalTransactions_Balance_NonNegative",
                    "[BalanceBefore] >= 0 AND [BalanceAfter] >= 0");

                table.HasCheckConstraint(
                    "CK_WithdrawalTransactions_Balance_Consistent",
                    "[BalanceAfter] = [BalanceBefore] - [Amount]");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.AccountId)
                .IsRequired();

            builder.Property(x => x.Amount)
                .HasPrecision(19, 4)
                .IsRequired();

            builder.Property(x => x.BalanceBefore)
                .HasPrecision(19, 4)
                .IsRequired();

            builder.Property(x => x.BalanceAfter)
                .HasPrecision(19, 4)
                .IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.CorrelationId)
                .HasMaxLength(100);

            // Account withdrawal history.
            builder.HasIndex(x => new
            {
                x.AccountId,
                x.CreatedAt
            });

            // Every withdrawal belongs to an account.
            builder.HasOne<BankAccount>()
                .WithMany()
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
