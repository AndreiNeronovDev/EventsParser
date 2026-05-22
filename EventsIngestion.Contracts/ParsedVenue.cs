namespace EventsIngestion.Contracts;

/// <summary>
/// Normalized venue data parsed from a source.
/// </summary>
public sealed record ParsedVenue
{
    /// <summary>
    /// Gets the stable venue identifier inside the source when available.
    /// </summary>
    public string? SourceVenueId { get; init; }

    /// <summary>
    /// Gets the venue display name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets an optional venue description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the venue public URL in the source system.
    /// </summary>
    public string? Url { get; init; }

    /// <summary>
    /// Gets the stage, hall, room, or sub-location inside the venue.
    /// </summary>
    public string? Stage { get; init; }

    /// <summary>
    /// Gets normalized venue address and coordinates.
    /// </summary>
    public ParsedAddress? Address { get; init; }

    /// <summary>
    /// Gets images associated with the venue.
    /// </summary>
    public IReadOnlyCollection<ParsedImage> Images { get; init; } = [];

    /// <summary>
    /// Gets venue social or website links.
    /// </summary>
    public IReadOnlyCollection<ParsedLink> Links { get; init; } = [];
}
