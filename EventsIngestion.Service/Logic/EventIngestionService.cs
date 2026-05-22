using EventsIngestion.Service.Abstraction;
using EventsIngestion.Service.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EventsIngestion.Service.Logic;

/// <summary>
/// Default event ingestion workflow service.
/// </summary>
public sealed class EventIngestionService(
    EventExtractorsRegistry registry,
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
        TEST(events);

        logger.LogInformation(
            "Event ingestion workflow extracted {ExtractedCount} events for source {SourceCode}.",
            events.Count,
            sourceCode);

        return new EventIngestionRunResult(
            sourceCode,
            events.Count,
            PublishedCount: 0);
    }

    private static void TEST(IReadOnlyCollection<Contracts.ParsedEventMessage> events)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        foreach (var message in events.Take(5))
        {
            Console.WriteLine("----- TEST parsed event -----");
            Console.WriteLine(JsonSerializer.Serialize(message, jsonOptions));
        }
    }
}
