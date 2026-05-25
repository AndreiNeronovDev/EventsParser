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
        IAmazonSQS sqsClient,
        IOptions<SqsOptions> options,
        ILogger<MessagePublisher> logger) : IMessagePublisher
    {
        private readonly SqsOptions _options = options.Value;

        /// <inheritdoc />
        public async Task PublishAsync(ParsedEventMessage message, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(_options.QueueUrl))
            {
                logger.LogError("SQS Queue URL is not configured. Skipping message publish.");
                return;
            }

            try
            {
                var messageBody = JsonSerializer.Serialize(message);

                var sendRequest = new SendMessageRequest
                {
                    QueueUrl = _options.QueueUrl,
                    MessageBody = messageBody
                };

                var response = await sqsClient.SendMessageAsync(sendRequest, cancellationToken);

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
            if (string.IsNullOrEmpty(_options.QueueUrl))
            {
                logger.LogWarning("SQS Queue URL is not configured. Skipping batch publish.");
                return 0;
            }

            if (messages.Count == 0)
            {
                logger.LogDebug("No messages to publish");
                return 0;
            }

            var sentMessagesCount = 0;

            var batches = messages.Chunk(_options.BatchSize);

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

                    var response = await sqsClient.SendMessageBatchAsync(batchRequest, cancellationToken);

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

    }
}
