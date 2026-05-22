using EventsIngestion.Contracts;
using EventsIngestion.Service.Abstraction;

namespace EventsIngestion.Service.Logic;

/// <summary>
/// Default extraction coordinator that delegates work to a source-specific extractor.
/// </summary>
public sealed class EventExtractionService(EventDataExtractorRegistry registry) : IEventExtractionService
{
    /// <inheritdoc />
    public Task<IReadOnlyCollection<ParsedEventMessage>> ExtractAsync(
        string sourceCode,
        CancellationToken cancellationToken)
    {
        var extractor = registry.GetRequired(sourceCode);
        return extractor.ExtractAsync(cancellationToken);
    }
}
