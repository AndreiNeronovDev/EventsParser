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
    IEventIngestionService ingestionService,
    IMessagePublisher messagePublisher,
    IHostApplicationLifetime applicationLifetime,
    ILogger<EventsIngestionWorker> logger) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await messagePublisher.EnsureReadyAsync(stoppingToken);

            var result = await ingestionService.RunAsync(options.SourceCode, stoppingToken);

            logger.LogInformation(
                "Finished events ingestion for source {SourceCode}. Extracted: {ExtractedCount}. Published: {PublishedCount}.",
                result.SourceCode,
                result.ExtractedCount,
                result.PublishedCount);
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
