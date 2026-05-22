namespace EventsIngestion.Contracts;

/// <summary>
/// Message published for one parsed event.
/// </summary>
public sealed record ParsedEventMessage
{
    /// <summary>
    /// Gets service, source, and tracing information for this message.
    /// </summary>
    public required ParsingMetadata Metadata { get; init; }

    /// <summary>
    /// Gets the normalized event data parsed from the source.
    /// </summary>
    public required ParsingPayload Event { get; init; }
}
