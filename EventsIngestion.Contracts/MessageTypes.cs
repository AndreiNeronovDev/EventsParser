namespace EventsIngestion.Contracts;

/// <summary>
/// Message type values used for queue filtering and downstream routing.
/// </summary>
public static class MessageTypes
{
    /// <summary>
    /// Parsed event message produced by the events ingestion service.
    /// </summary>
    public const string ParsedEvent = "ParsedEvent";
}
