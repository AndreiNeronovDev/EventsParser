namespace EventsIngestion.Contracts;

/// <summary>
/// Source classification value such as segment, category, genre, subgenre, type, or discipline.
/// </summary>
public sealed record ParsedClassification
{
    /// <summary>
    /// Gets the source classification identifier when available.
    /// </summary>
    public string? SourceId { get; init; }

    /// <summary>
    /// Gets the classification type, such as segment, category, genre, or subgenre.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// Gets the source classification display name.
    /// </summary>
    public required string Name { get; init; }
}
