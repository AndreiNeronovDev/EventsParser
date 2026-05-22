using EventsIngestion.Service.Abstraction;
using EventsIngestion.Service.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventsIngestion.Service.Logic;

/// <summary>
/// Runs one ingestion job inside the Fargate task and then stops the host.
/// </summary>
public sealed class EventsIngestionWorker(
    IngestionTaskOptions options,
    IEventExtractionService extractionService,
    IHostApplicationLifetime applicationLifetime,
    ILogger<EventsIngestionWorker> logger) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.LogInformation(
                "Starting events ingestion for source {SourceCode}.",
                options.SourceCode);

            var events = await extractionService.ExtractAsync(options.SourceCode, stoppingToken);

            logger.LogInformation(
                "Finished events ingestion for source {SourceCode}. Parsed events: {EventCount}.",
                options.SourceCode,
                events.Count);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Events ingestion was cancelled.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Events ingestion failed.");
            Environment.ExitCode = 1;
        }
        finally
        {
            applicationLifetime.StopApplication();
        }
    }
}
