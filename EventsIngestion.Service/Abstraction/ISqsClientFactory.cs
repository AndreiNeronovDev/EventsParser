using Amazon.SQS;

namespace EventsIngestion.Service.Abstraction;

/// <summary>
/// Creates SQS clients from runtime configuration.
/// </summary>
public interface ISqsClientFactory
{
    /// <summary>
    /// Creates an SQS client for the configured AWS or local-compatible endpoint.
    /// </summary>
    IAmazonSQS CreateClient();
}
