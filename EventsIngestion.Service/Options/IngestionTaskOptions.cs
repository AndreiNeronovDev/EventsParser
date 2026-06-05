using Microsoft.Extensions.Configuration;

namespace EventsIngestion.Service.Options;

/// <summary>
/// Options supplied to one EventBridge-triggered Fargate task execution.
/// </summary>
public sealed record IngestionTaskOptions
{
    /// <summary>
    /// Gets the source code used to select an event data extractor.
    /// </summary>
    public required string SourceCode { get; init; }

    /// <summary>
    /// Builds task options from host configuration supplied by the EventBridge/ECS task target.
    /// </summary>
    public static IngestionTaskOptions FromConfiguration(IConfiguration configuration)
    {
        var sourceCode = configuration["SourceCode"] ?? configuration["SOURCE_CODE"];
        if (string.IsNullOrWhiteSpace(sourceCode))
            throw new InvalidOperationException("SourceCode configuration value is required.");

        return new IngestionTaskOptions { SourceCode = sourceCode };
    }
}
