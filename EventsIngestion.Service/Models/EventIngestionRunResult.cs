namespace EventsIngestion.Service.Models;

/// <summary>
/// Summary of one event ingestion workflow run.
/// </summary>
public sealed record EventIngestionRunResult(
    string SourceCode,
    int ExtractedCount,
    int PublishedCount);
