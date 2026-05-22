namespace EventsIngestion.Contracts;

/// <summary>
/// Normalized attraction or artist data attached to an event.
/// </summary>
public sealed record ParsedAttraction
{
    /// <summary>
    /// Gets the stable attraction identifier inside the source when available.
    /// </summary>
    public string? SourceAttractionId { get; init; }

    /// <summary>
    /// Gets the attraction or artist name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the attraction or artist public URL.
    /// </summary>
    public string? Url { get; init; }

    /// <summary>
    /// Gets the attraction website when distinct from the source URL.
    /// </summary>
    public string? Website { get; init; }

    /// <summary>
    /// Gets images associated with the attraction.
    /// </summary>
    public IReadOnlyCollection<ParsedImage> Images { get; init; } = [];

    /// <summary>
    /// Gets social or website links associated with the attraction.
    /// </summary>
    public IReadOnlyCollection<ParsedLink> Links { get; init; } = [];

    /// <summary>
    /// Gets source classification values associated with the attraction.
    /// </summary>
    public IReadOnlyCollection<ParsedClassification> Classifications { get; init; } = [];
}
