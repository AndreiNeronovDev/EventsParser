using EventsIngestion.Contracts;

namespace EventsIngestion.Service.Abstraction;

/// <summary>
/// Extracts normalized event data from one external source.
/// </summary>
public interface IEventDataExtractor
{
    /// <summary>
    /// Gets the source code used to select this extractor for a scheduled run.
    /// </summary>
    string SourceCode { get; }

    /// <summary>
    /// Gathers source data and maps it to normalized parsed event messages.
    /// </summary>
    Task<IReadOnlyCollection<ParsedEventMessage>> ExtractAsync(CancellationToken cancellationToken);
}
