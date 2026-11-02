using Bank.Domain.Entities;
using Bank.Infrastructure.Persistence.Idempotency;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bank.Infrastructure.Persistence.Configurations
{
    public sealed class IdempotencyRecordConfiguration
        : IEntityTypeConfiguration<IdempotencyRecord>
    {
        public void Configure(
            EntityTypeBuilder<IdempotencyRecord> builder)
        {
            builder.ToTable("IdempotencyRecords");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.IdempotencyKey)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.RequestHash)
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            // Prevent duplicate operations per account.
            builder.HasIndex(x => new
            {
                x.AccountId,
                x.IdempotencyKey
            })
            .IsUnique()
            .HasDatabaseName(
                "UX_IdempotencyRecords_AccountId_Key");

            // Each idempotency record references
            // an actual financial withdrawal.
            builder.HasOne<WithdrawalTransaction>()
                .WithMany()
                .HasForeignKey(x => x.WithdrawalId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<BankAccount>()
                .WithMany()
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
