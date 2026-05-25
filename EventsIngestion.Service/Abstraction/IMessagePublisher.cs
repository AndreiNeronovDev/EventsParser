using EventsIngestion.Contracts;

namespace EventsIngestion.Service.Abstraction;

/// <summary>
/// Abstraction of the SQS messages publisher 
/// </summary>
public interface IMessagePublisher
{
    /// <summary>
    /// Publish a single message 
    /// </summary>
    /// <param name="message">Message to send</param>
    Task PublishAsync(ParsedEventMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Divide the collection to the batches and send them 
    /// </summary>
    /// <param name="messages">Messages to be sent</param>
    /// <returns>Number of total sent messages</returns>
    Task<int> PublishBatchAsync(IReadOnlyCollection<ParsedEventMessage> messages, CancellationToken cancellationToken = default);
}