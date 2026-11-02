using Bank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bank.Infrastructure.Persistence.Configurations
{
    public sealed class BankAccountConfiguration
    : IEntityTypeConfiguration<BankAccount>
    {
        public void Configure(
            EntityTypeBuilder<BankAccount> builder)
        {
            builder.ToTable("Accounts", table =>
            {
                table.HasCheckConstraint(
                    "CK_Accounts_Balance_NonNegative",
                    "[Balance] >= 0");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Balance)
                .HasPrecision(19, 4)
                .IsRequired();

            builder.Property(x => x.Version)
                .IsConcurrencyToken()
                .IsRequired();
        }
    }
}
