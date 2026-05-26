using Amazon;
using Amazon.SQS;
using EventsIngestion.Service.Abstraction;
using EventsIngestion.Service.Options;
using Microsoft.Extensions.Options;

namespace EventsIngestion.Service.Logic;

/// <summary>
/// Creates Amazon SQS clients for production AWS endpoints or local-compatible endpoints.
/// </summary>
public sealed class SqsClientFactory(IOptions<SqsOptions> options) : ISqsClientFactory
{
    private readonly SqsOptions _options = options.Value;

    /// <inheritdoc />
    public IAmazonSQS CreateClient()
    {
        var config = new AmazonSQSConfig
        {
            RegionEndpoint = RegionEndpoint.GetBySystemName(_options.Region)
        };

        if (!string.IsNullOrWhiteSpace(_options.ServiceUrl))
            config.ServiceURL = _options.ServiceUrl;

        return new AmazonSQSClient(config);
    }
}
