using EventsIngestion.Service.Models;

namespace EventsIngestion.Service.Abstraction;

/// <summary>
/// Runs the full event ingestion workflow for a scheduled source run.
/// </summary>
public interface IEventIngestionService
{
    /// <summary>
    /// Resolves the source extractor, runs parsing, and performs downstream ingestion steps.
    /// </summary>
    Task<EventIngestionRunResult> RunAsync(
        string sourceCode,
        CancellationToken cancellationToken);
}
