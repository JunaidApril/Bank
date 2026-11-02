using Amazon.SimpleNotificationService;
using Amazon.Extensions.NETCore.Setup;
using Bank.Application.Interfaces;
using Bank.Infrastructure.Messaging;
using Bank.Infrastructure.Messaging.Configurations;
using Bank.Infrastructure.Messaging.Implementations;
using Bank.Infrastructure.Messaging.Interfaces;
using Bank.Infrastructure.Observability;
using Bank.Infrastructure.Persistence;
using Bank.Infrastructure.Persistence.Outbox;
using Bank.Infrastructure.Persistence.Repositories;
using Bank.Infrastructure.Persistence.Idempotency;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bank.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Database
            services.AddDbContext<BankingDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString(
                        "BankingDatabase")));

            // Unit of Work
            services.AddScoped<IUnitOfWork>(
                provider => provider.GetRequiredService<
                    BankingDbContext>());

            // Repositories
            services.AddScoped<IAccountRepository, AccountRepository>();

            services.AddScoped<IWithdrawalRepository, WithdrawalRepository>();

            services.AddScoped<IOutboxRepository, OutboxRepository>();

            services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();

            services.AddScoped<IOutboxReplayService, OutboxReplayService>();

            // Time abstraction
            services.AddSingleton<TimeProvider>(
                TimeProvider.System);

            // Outbox configuration
            services
                .AddOptions<OutboxOptions>()
                .Bind(configuration.GetSection(
                    OutboxOptions.SectionName))
                .Validate(x => x.BatchSize > 0 &&
                               x.BatchSize <= 1000)
                .Validate(x => x.MaximumAttempts > 0)
                .Validate(x => x.LeaseDurationMinutes > 0)
                .Validate(x => x.PollingIntervalSeconds > 0)
                .Validate(x => x.RetentionDays > 0)
                .Validate(x => x.DeadLetterRetentionDays > 0)
                .Validate(x => x.PublishTimeoutSeconds > 0)
                .Validate(x => x.LeaseSafetyBufferSeconds > 0)
                .Validate(
                    x => TimeSpan.FromMinutes(
                        x.LeaseDurationMinutes) >
                        TimeSpan.FromSeconds(
                            (long)x.BatchSize *
                            x.PublishTimeoutSeconds +
                            x.LeaseSafetyBufferSeconds),
                    "Outbox lease must exceed the maximum batch publishing window.")
                .ValidateOnStart();

            // AWS SNS
            services.AddDefaultAWSOptions(
                configuration.GetAWSOptions());

            services.AddAWSService<
                IAmazonSimpleNotificationService>();

            services.AddScoped<
                ISnsEventPublisher,
                SnsEventPublisher>();

            // Outbox processing
            services.AddScoped<
                IOutboxProcessor,
                OutboxBatchProcessor>();

            services.AddHostedService<OutboxProcessor>();

            // Retention processing
            services.AddHostedService<
                OutboxRetentionProcessor>();

            // Metrics
            services.AddSingleton<OutboxMetrics>();

            return services;
        }
    }
}
