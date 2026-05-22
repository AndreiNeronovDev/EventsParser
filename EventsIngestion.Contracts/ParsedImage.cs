namespace EventsIngestion.Contracts;

/// <summary>
/// Normalized image reference parsed from a source.
/// </summary>
public sealed record ParsedImage
{
    /// <summary>
    /// Gets the image URL.
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// Gets the image width in pixels when available.
    /// </summary>
    public int? Width { get; init; }

    /// <summary>
    /// Gets the image height in pixels when available.
    /// </summary>
    public int? Height { get; init; }

    /// <summary>
    /// Gets the source ratio label or aspect ratio value when available.
    /// </summary>
    public string? Ratio { get; init; }

    /// <summary>
    /// Gets the source image identifier when available.
    /// </summary>
    public string? SourceImageId { get; init; }
}
