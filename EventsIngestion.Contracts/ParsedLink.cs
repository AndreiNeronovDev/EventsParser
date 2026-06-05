namespace EventsIngestion.Contracts;

/// <summary>
/// Normalized external link parsed from a source.
/// </summary>
public sealed record ParsedLink
{
    /// <summary>
    /// Gets the link type, such as website, facebook, instagram, spotify, or youtube.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// Gets the link URL.
    /// </summary>
    public required string Url { get; init; }
}
