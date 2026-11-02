using Bank.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bank.Infrastructure.Persistence.Configurations
{
    public sealed class OutboxMessageConfiguration
     : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(
            EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("OutboxMessages");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EventType)
                .HasMaxLength(250)
                .IsRequired();

            builder.Property(x => x.Payload)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(x => x.CorrelationId)
                .HasMaxLength(100);

            builder.Property(x => x.LastError)
                .HasMaxLength(2000);

            builder.Property(x => x.Attempts)
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(x => x.LockId)
                .IsConcurrencyToken();

            builder.Property(x => x.OccurredAt)
                .IsRequired();

            // Supports retrieving eligible messages.
            builder.HasIndex(x => new
            {
                x.PublishedAt,
                x.DeadLetteredAt,
                x.NextAttemptAt,
                x.OccurredAt
            });

            // Supports retrieving messages by worker lease.
            builder.HasIndex(x => x.LockId);
        }
    }
}
