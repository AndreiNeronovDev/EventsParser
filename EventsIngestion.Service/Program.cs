using EventsIngestion.Service.Abstraction;
using EventsIngestion.Service.Logic;
using EventsIngestion.Service.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ ";
    options.UseUtcTimestamp = true;
});

builder.Services.Configure<HostOptions>(options =>
{
    var shutdownTimeoutSeconds = builder.Configuration.GetValue(
        "Host:ShutdownTimeoutSeconds",
        30);

    options.ShutdownTimeout = TimeSpan.FromSeconds(shutdownTimeoutSeconds);
});

builder.Services.AddSingleton(_ => IngestionTaskOptions.FromConfiguration(builder.Configuration));
builder.Services.AddSingleton(_ => EventDataExtractorRegistry.CreateDefault());
builder.Services.AddSingleton<IEventExtractionService, EventExtractionService>();
builder.Services.AddHostedService<EventsIngestionWorker>();

await builder.Build().RunAsync();
