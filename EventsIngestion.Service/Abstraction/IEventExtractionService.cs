using EventsIngestion.Contracts;

namespace EventsIngestion.Service.Abstraction;

/// <summary>
/// Coordinates event extraction for a scheduled source run.
/// </summary>
public interface IEventExtractionService
{
    /// <summary>
    /// Resolves the source extractor by source code and returns normalized parsed event DTOs.
    /// </summary>
    Task<IReadOnlyCollection<ParsedEventMessage>> ExtractAsync(
        string sourceCode,
        CancellationToken cancellationToken);
}
