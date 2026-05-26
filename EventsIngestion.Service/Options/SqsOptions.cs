namespace EventsIngestion.Service.Options;

/// <summary>
/// SQS publisher settings.
/// </summary>
public class SqsOptions
{
    /// <summary>
    /// Gets or sets the target queue URL.
    /// </summary>
    public string QueueUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the AWS region used by the SQS client.
    /// </summary>
    public string Region { get; set; } = "eu-west-1";

    /// <summary>
    /// Gets or sets an optional custom SQS service endpoint, such as a LocalStack endpoint.
    /// </summary>
    public string ServiceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the requested publish batch size.
    /// </summary>
    public int BatchSize { get; set; } = 10;

    /// <summary>
    /// Gets or sets the maximum retry count reserved for publisher retry behavior.
    /// </summary>
    public int MaxRetries { get; set; } = 3;
}
