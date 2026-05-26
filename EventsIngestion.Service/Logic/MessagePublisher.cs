using Amazon.SQS;
using Amazon.SQS.Model;
using EventsIngestion.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using EventsIngestion.Service.Abstraction;
using EventsIngestion.Service.Options;

namespace EventsIngestion.Service.Logic
{
    internal class MessagePublisher(
        ISqsClientFactory sqsClientFactory,
        IOptions<SqsOptions> options,
        ILogger<MessagePublisher> logger)
        : IMessagePublisher
    {
        // AWS SQS SendMessageBatch supports no more than 10 messages per request.
        private const int MaxSqsBatchSize = 10;

        private readonly IAmazonSQS _sqsClient = sqsClientFactory.CreateClient();

        private readonly SqsOptions _options = options.Value;

        /// <inheritdoc />
        public async Task EnsureReadyAsync(CancellationToken cancellationToken = default)
        {
            EnsureQueueUrlConfigured();

            var request = new GetQueueAttributesRequest
            {
                QueueUrl = _options.QueueUrl,
                AttributeNames = ["QueueArn"]
            };

            var attributes = await _sqsClient.GetQueueAttributesAsync(request, cancellationToken);

            logger.LogInformation("SQS queue connection check succeeded.");
        }

        /// <inheritdoc />
        public async Task PublishAsync(ParsedEventMessage message, CancellationToken cancellationToken = default)
        {
            EnsureQueueUrlConfigured();

            try
            {
                var messageBody = JsonSerializer.Serialize(message);

                var sendRequest = new SendMessageRequest
                {
                    QueueUrl = _options.QueueUrl,
                    MessageBody = messageBody
                };

                var response = await _sqsClient.SendMessageAsync(sendRequest, cancellationToken);

                if ((int)response.HttpStatusCode >= 200 && (int)response.HttpStatusCode < 300)
                {
                    logger.LogInformation($"Message {message.Metadata.MessageId} successfully published");
                }
                else
                {
                    logger.LogWarning(
                        $"Failed to publish message {message.Metadata.MessageId}. Status code: {response.HttpStatusCode}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    $"Failed to publish message  {message.Metadata.MessageId}");
            }
        }

        /// <inheritdoc />
        public async Task<int> PublishBatchAsync(IReadOnlyCollection<ParsedEventMessage> messages, CancellationToken cancellationToken = default)
        {
            EnsureQueueUrlConfigured();

            if (messages.Count == 0)
            {
                logger.LogDebug("No messages to publish");
                return 0;
            }

            var sentMessagesCount = 0;

            var batches = messages.Chunk(GetBatchSize(_options.BatchSize));

            foreach (var batch in batches)
            {
                try
                {
                    var entries = batch.Select((msg, index) => new SendMessageBatchRequestEntry
                    {
                        Id = index.ToString(),
                        MessageBody = JsonSerializer.Serialize(msg)
                    }).ToList();

                    var batchRequest = new SendMessageBatchRequest
                    {
                        QueueUrl = _options.QueueUrl,
                        Entries = entries
                    };

                    var response = await _sqsClient.SendMessageBatchAsync(batchRequest, cancellationToken);

                    if (response.Successful.Count > 0)
                    {
                        sentMessagesCount += response.Successful.Count;

                        logger.LogInformation(
                            "Successfully published batch of {Count} messages to SQS",
                            response.Successful.Count);
                    }

                    if (response.Failed.Count > 0)
                    {
                        logger.LogWarning(
                            "Failed to publish {Count} messages in batch. Errors: {Errors}",
                            response.Failed.Count,
                            string.Join(", ", response.Failed.Select(f => $"{f.Id}: {f.Message}")));
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to publish batch of {Count} messages", batch.Count());
                }
            }

            return sentMessagesCount;
        }

        private static int GetBatchSize(int configuredBatchSize)
        {
            var size = configuredBatchSize > 0 ? configuredBatchSize : 1;

            return Math.Min(size, MaxSqsBatchSize);
        }

        private void EnsureQueueUrlConfigured()
        {
            if (string.IsNullOrWhiteSpace(_options.QueueUrl))
                throw new InvalidOperationException("SQS QueueUrl configuration value is required.");
        }

    }
}
