namespace EventsIngestion.Contracts;

/// <summary>
/// Normalized performer or lineup item parsed from a source.
/// </summary>
public sealed record ParsedLineupItem
{
    /// <summary>
    /// Gets the performer or lineup item name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the source performer identifier when available.
    /// </summary>
    public string? SourceId { get; init; }

    /// <summary>
    /// Gets the performer URL when available.
    /// </summary>
    public string? Url { get; init; }

    /// <summary>
    /// Gets the planned local start time text for this lineup item.
    /// </summary>
    public string? StartTimeText { get; init; }

    /// <summary>
    /// Gets images associated with the performer.
    /// </summary>
    public IReadOnlyCollection<ParsedImage> Images { get; init; } = [];

    /// <summary>
    /// Gets social or website links associated with the performer.
    /// </summary>
    public IReadOnlyCollection<ParsedLink> Links { get; init; } = [];
}
