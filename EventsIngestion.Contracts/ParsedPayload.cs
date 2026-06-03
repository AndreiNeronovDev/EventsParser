namespace EventsIngestion.Contracts;

/// <summary>
/// Normalized event data parsed from an external source.
/// </summary>
public sealed record ParsedPayload
{
    /// <summary>
    /// Gets the event title or display name.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets an optional subtitle, heading, oneliner, or short source-specific label.
    /// </summary>
    public string? Subtitle { get; init; }

    /// <summary>
    /// Gets the normalized event description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets optional extra notes from the source, such as warnings, door policies, or "please note" text.
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Gets the event start instant when it can be parsed as a concrete date and time.
    /// </summary>
    public DateTimeOffset? StartsAt { get; init; }

    /// <summary>
    /// Gets the source-local event date at midnight when a calendar date can be parsed.
    /// </summary>
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// Gets the event end instant when it is available from the source.
    /// </summary>
    public DateTimeOffset? EndsAt { get; init; }

    /// <summary>
    /// Gets the door opening instant when it is available from the source.
    /// </summary>
    public DateTimeOffset? DoorsOpenAt { get; init; }

    /// <summary>
    /// Gets the source date text when the source cannot yet be normalized into StartsAt.
    /// </summary>
    public string? DateText { get; init; }

    /// <summary>
    /// Gets the source start time text when the source cannot yet be normalized into StartsAt.
    /// </summary>
    public string? StartTimeText { get; init; }

    /// <summary>
    /// Gets the source end time text when the source cannot yet be normalized into EndsAt.
    /// </summary>
    public string? EndTimeText { get; init; }

    /// <summary>
    /// Gets the source doors-open time text when the source cannot yet be normalized into DoorsOpenAt.
    /// </summary>
    public string? DoorsOpenText { get; init; }

    /// <summary>
    /// Gets the source or IANA time zone identifier when available.
    /// </summary>
    public string? TimeZoneId { get; init; }

    /// <summary>
    /// Gets the current event status reported by the source.
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    /// Gets the canonical public URL for this event in the source system.
    /// </summary>
    public string? ExternalUrl { get; init; }

    /// <summary>
    /// Gets an optional seat map image or page URL.
    /// </summary>
    public string? SeatmapUrl { get; init; }

    /// <summary>
    /// Gets the parsed venue and location data.
    /// </summary>
    public ParsedVenue? Venue { get; init; }

    /// <summary>
    /// Gets normalized event images.
    /// </summary>
    public IReadOnlyCollection<ParsedImage> Images { get; init; } = [];

    /// <summary>
    /// Gets normalized ticket offers.
    /// </summary>
    public IReadOnlyCollection<ParsedTicket> Tickets { get; init; } = [];

    /// <summary>
    /// Gets performer, artist, or lineup items parsed from the source.
    /// </summary>
    public IReadOnlyCollection<ParsedLineupItem> Lineup { get; init; } = [];

    /// <summary>
    /// Gets source category, segment, genre, or subgenre values before internal classification mapping.
    /// </summary>
    public IReadOnlyCollection<ParsedClassification> Classifications { get; init; } = [];

    /// <summary>
    /// Gets normalized genre labels parsed or inferred from the source.
    /// </summary>
    public IReadOnlyCollection<string> Genres { get; init; } = [];

    /// <summary>
    /// Gets external artist or attraction data attached to the event.
    /// </summary>
    public IReadOnlyCollection<ParsedAttraction> Attractions { get; init; } = [];

    /// <summary>
    /// Gets whether the source indicates the event is legally age-restricted.
    /// </summary>
    public bool? LegalAgeEnforced { get; init; }
}
