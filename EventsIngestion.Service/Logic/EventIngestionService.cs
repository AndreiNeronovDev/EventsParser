using EventsIngestion.Service.Abstraction;
using EventsIngestion.Service.Models;
using Microsoft.Extensions.Logging;

namespace EventsIngestion.Service.Logic;

/// <summary>
/// Default event ingestion workflow service.
/// </summary>
public sealed class EventIngestionService(
    EventExtractorsRegistry registry,
    IMessagePublisher messagePublisher,
    ILogger<EventIngestionService> logger) : IEventIngestionService
{
    /// <inheritdoc />
    public async Task<EventIngestionRunResult> RunAsync(
        string sourceCode,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting event ingestion workflow for source {SourceCode}.", sourceCode);

        var extractor = registry.GetExtractor(sourceCode);
        var events = await extractor.ExtractAsync(cancellationToken);

        logger.LogInformation(
            "Event ingestion workflow extracted {ExtractedCount} events for source {SourceCode}.",
            events.Count,
            sourceCode);

        var publishedCount = await messagePublisher.PublishBatchAsync(events, cancellationToken);

        return new EventIngestionRunResult(
            sourceCode,
            events.Count,
            PublishedCount: publishedCount);
    }
}
