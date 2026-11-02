
using Bank.Application.Exceptions;
using Bank.Application.Interfaces;
using Bank.Domain.Entities;
using Bank.Infrastructure.Persistence.Idempotency;
using Bank.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Bank.Infrastructure.Persistence
{
    public sealed class BankingDbContext
        : DbContext, IUnitOfWork
    {
        public BankingDbContext(
            DbContextOptions<BankingDbContext> options)
            : base(options)
        {
        }

        public DbSet<BankAccount> Accounts =>
            Set<BankAccount>();

        public DbSet<OutboxMessage> OutboxMessages =>
            Set<OutboxMessage>();

        public DbSet<WithdrawalTransaction> WithdrawalTransactions =>
            Set<WithdrawalTransaction>();

        public DbSet<IdempotencyRecord> IdempotencyRecords =>
            Set<IdempotencyRecord>();

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(BankingDbContext).Assembly);
        }

        public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(
            CancellationToken cancellationToken)
        {
            var transaction = await Database.BeginTransactionAsync(
                cancellationToken);

            return new EfUnitOfWorkTransaction(transaction);
        }

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new ConcurrencyConflictException(
                    "The account was modified by another request. Please retry.",
                    ex);
            }
        }
    }
}
