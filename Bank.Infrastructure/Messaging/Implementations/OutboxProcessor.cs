using Bank.Infrastructure.Messaging.Configurations;
using Bank.Infrastructure.Messaging.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bank.Infrastructure.Messaging.Implementations
{
    public sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessor> logger,
    IOptions<OutboxOptions> options)
    : BackgroundService
    {
        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            logger.LogInformation(
                "Outbox background processor started.");

            var interval = TimeSpan.FromSeconds(
                options.Value.PollingIntervalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = scopeFactory.CreateScope();

                    var processor = scope.ServiceProvider
                        .GetRequiredService<IOutboxProcessor>();

                    await processor.ProcessBatchAsync(
                        stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    logger.LogError(
                        exception,
                        "Error processing outbox messages.");
                }

                try
                {
                    await Task.Delay(
                        interval,
                        stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }

            logger.LogInformation(
                "Outbox background processor stopped.");
        }
    }
}
