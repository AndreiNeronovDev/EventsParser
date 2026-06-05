using Amazon;
using Amazon.SQS;
using EventsIngestion.Service.Logic;
using EventsIngestion.Service.Options;

namespace EventsIngestion.Service.Tests;

public sealed class SqsClientFactoryTests
{
    [Test]
    public void CreateClient_UsesConfiguredRegion()
    {
        var options = new SqsOptions
        {
            Region = "eu-central-1"
        };

        using var client = CreateClient(options);
        var config = (AmazonSQSConfig)client.Config;

        Assert.That(config.RegionEndpoint, Is.EqualTo(RegionEndpoint.EUCentral1));
    }

    [Test]
    public void CreateClient_UsesServiceUrl_WhenConfigured()
    {
        var options = new SqsOptions
        {
            Region = "eu-west-1",
            ServiceUrl = "http://localstack:4566"
        };

        using var client = CreateClient(options);
        var config = (AmazonSQSConfig)client.Config;

        Assert.That(config.ServiceURL, Is.EqualTo("http://localstack:4566/"));
    }

    [Test]
    public void CreateClient_LeavesServiceUrlEmpty_WhenNotConfigured()
    {
        var options = new SqsOptions
        {
            Region = "eu-west-1"
        };

        using var client = CreateClient(options);
        var config = (AmazonSQSConfig)client.Config;

        Assert.That(config.ServiceURL, Is.Null.Or.Empty);
    }

    private static AmazonSQSClient CreateClient(SqsOptions options)
    {
        var factory = new SqsClientFactory(Microsoft.Extensions.Options.Options.Create(options));

        return (AmazonSQSClient)factory.CreateClient();
    }
}
