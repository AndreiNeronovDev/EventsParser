using EventsIngestion.Service.Abstraction;
using Microsoft.Extensions.Logging;

namespace EventsIngestion.Service.Logic;

/// <summary>
/// Stores source extractors and resolves them by source code.
/// </summary>
public sealed class EventExtractorsRegistry
{
    private readonly IReadOnlyDictionary<string, IEventDataExtractor> _extractors;

    private readonly ILogger<EventExtractorsRegistry> _logger;

    /// <summary>
    /// Creates the default registry from source extractors registered in dependency injection.
    /// </summary>
    public EventExtractorsRegistry(
        IEnumerable<IEventDataExtractor> extractors,
        ILogger<EventExtractorsRegistry> logger)
    {
        _logger = logger;
        _extractors = extractors.ToDictionary(
            extractor => extractor.SourceCode,
            StringComparer.OrdinalIgnoreCase);

    }

    /// <summary>
    /// Returns the extractor for the requested source code or fails the run if none is registered.
    /// </summary>
    public IEventDataExtractor GetExtractor(string sourceCode)
    {
        if (_extractors.TryGetValue(sourceCode, out var extractor))
            return extractor;

        _logger.LogError("No event data extractor registered for source: {SourceCode}.", sourceCode);
        throw new InvalidOperationException($"No event data extractor registered for source: '{sourceCode}'.");
    }
}
