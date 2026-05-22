using EventsIngestion.Service.Abstraction;

namespace EventsIngestion.Service.Logic;

/// <summary>
/// Stores source extractors and resolves them by source code.
/// </summary>
public sealed class EventDataExtractorRegistry
{
    private readonly IReadOnlyDictionary<string, IEventDataExtractor> _extractors;

    /// <summary>
    /// Creates a registry from the available source extractors.
    /// </summary>
    public EventDataExtractorRegistry(IEnumerable<IEventDataExtractor> extractors)
    {
        _extractors = extractors.ToDictionary(
            extractor => extractor.SourceCode,
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Creates the default extractor registry for the service.
    /// </summary>
    public static EventDataExtractorRegistry CreateDefault()
        => new(
        [
            new FirstDummyEventDataExtractor(),
            new SecondDummyEventDataExtractor()
        ]);

    /// <summary>
    /// Returns the extractor for the requested source code or fails the run if none is registered.
    /// </summary>
    public IEventDataExtractor GetRequired(string sourceCode)
    {
        if (_extractors.TryGetValue(sourceCode, out var extractor))
            return extractor;

        throw new InvalidOperationException($"No event data extractor registered for source '{sourceCode}'.");
    }
}
