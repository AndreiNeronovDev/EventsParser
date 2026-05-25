namespace EventsIngestion.Service.Options;

/// <summary>
/// SQS publisher settings.
/// </summary>
public class SqsOptions
{
    public string QueueUrl { get; set; } = string.Empty;
    public string Region { get; set; } = "eu-west-1";
    public int BatchSize { get; set; } = 10;
    public int MaxRetries { get; set; } = 3;
}
