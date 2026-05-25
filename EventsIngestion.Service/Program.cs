using Amazon;
using Amazon.SQS;
using EventsIngestion.Service.Abstraction;
using EventsIngestion.Service.Logic;
using EventsIngestion.Service.Logic.Extractors;
using EventsIngestion.Service.Options;
using EventsIngestion.Source.Muziekladder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<HostOptions>(options =>
{
    var shutdownTimeoutSeconds = builder.Configuration.GetValue(
        "Host:ShutdownTimeoutSeconds",
        30);

    options.ShutdownTimeout = TimeSpan.FromSeconds(shutdownTimeoutSeconds);
});

builder.Services.Configure<SqsOptions>(builder.Configuration.GetSection("Sqs"));

//Adding extractors: 
builder.Services.AddSingleton<IEventDataExtractor, MLadderEventExtractor>();

builder.Services.AddSingleton(_ => IngestionTaskOptions.FromConfiguration(builder.Configuration));
builder.Services.AddSingleton<IAmazonSQS>(sp =>
{
    var options = sp.GetRequiredService<IOptions<SqsOptions>>().Value;
    return new AmazonSQSClient(RegionEndpoint.GetBySystemName(options.Region));
});
builder.Services.AddSingleton<MLadderEventReader>();
builder.Services.AddSingleton<EventExtractorsRegistry>();
builder.Services.AddSingleton<IEventIngestionService, EventIngestionService>();
builder.Services.AddSingleton<IMessagePublisher, MessagePublisher>();
builder.Services.AddHostedService<EventsIngestionWorker>();

await builder.Build().RunAsync();
