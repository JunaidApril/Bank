using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Bank.Infrastructure.Messaging.Configurations;
using Bank.Infrastructure.Messaging.Interfaces;
using Bank.Infrastructure.Persistence.Outbox;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bank.Infrastructure.Messaging.Implementations
{
    public sealed class SnsEventPublisher(
        IAmazonSimpleNotificationService snsClient,
        IConfiguration configuration,
        ILogger<SnsEventPublisher> logger,
        IOptions<OutboxOptions> options)
        : ISnsEventPublisher
    {
        private readonly string _topicArn =
            configuration["AWS:SNS:WithdrawalTopicArn"]
            ?? throw new InvalidOperationException(
                "SNS topic ARN is not configured.");

        public async Task PublishAsync(
            OutboxMessage message,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(message);

            using var timeoutCts =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken);

            timeoutCts.CancelAfter(
                TimeSpan.FromSeconds(
                    options.Value.PublishTimeoutSeconds));

            var request = new PublishRequest
            {
                TopicArn = _topicArn,
                Message = message.Payload,
                MessageAttributes =
                {
                    ["EventType"] = new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = message.EventType
                    },

                    ["EventId"] = new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = message.Id.ToString()
                    },

                    ["CorrelationId"] = new MessageAttributeValue
                    {
                        DataType = "String",
                        StringValue = message.CorrelationId
                            ?? message.Id.ToString()
                    }
                }
            };

            var response = await snsClient.PublishAsync(
                request,
                timeoutCts.Token);

            logger.LogInformation(
                "Published event {EventId}. " +
                "Type: {EventType}. " +
                "CorrelationId: {CorrelationId}. " +
                "SNS MessageId: {SnsMessageId}",
                message.Id,
                message.EventType,
                message.CorrelationId,
                response.MessageId);
        }
    }
}
