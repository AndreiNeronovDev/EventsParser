namespace EventsIngestion.Contracts;

/// <summary>
/// Metadata attached to a parsed event message for tracing, routing, and idempotency.
/// </summary>
public sealed record ParsedMetadata
{
    /// <summary>
    /// Version of the message contract, used by consumers to handle future shape changes safely.
    /// </summary>
    public required string SchemaVersion { get; init; }

    /// <summary>
    /// Unique identifier of this specific queue message, used for tracing, logging, and deduplication.
    /// </summary>
    public required Guid MessageId { get; init; }

    /// <summary>
    /// Unique identifier of one ingestion run; all messages produced by the same Fargate execution share this value.
    /// </summary>
    public required Guid RunId { get; init; }

    /// <summary>
    /// Stable identifier of the external data source, used for routing, filtering, debugging, and source-specific logic.
    /// </summary>
    public required string SourceCode { get; init; }

    /// <summary>
    /// Stable event identifier inside the source, usually combined with SourceCode for idempotent upserts.
    /// </summary>
    public required string SourceEventId { get; init; }

    /// <summary>
    /// UTC timestamp when the ingestion service produced this message, used for freshness checks and replay diagnostics.
    /// </summary>
    public required DateTimeOffset ProducedAtUtc { get; init; }
}
