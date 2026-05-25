using EventsIngestion.Service.Options;
using Microsoft.Extensions.Configuration;

namespace EventsIngestion.Service.Tests;

public sealed class IngestionTaskOptionsTests
{
    [Test]
    public void FromConfiguration_ReadsSourceCode()
    {
        var configuration = CreateConfiguration(("SourceCode", "muziekladder"));

        var options = IngestionTaskOptions.FromConfiguration(configuration);

        Assert.That(options.SourceCode, Is.EqualTo("muziekladder"));
    }

    [Test]
    public void FromConfiguration_ReadsEnvironmentStyleSourceCode()
    {
        var configuration = CreateConfiguration(("SOURCE_CODE", "ticketmaster"));

        var options = IngestionTaskOptions.FromConfiguration(configuration);

        Assert.That(options.SourceCode, Is.EqualTo("ticketmaster"));
    }

    [Test]
    public void FromConfiguration_Throws_WhenSourceCodeIsMissing()
    {
        var configuration = CreateConfiguration();

        var exception = Assert.Throws<InvalidOperationException>(
            () => IngestionTaskOptions.FromConfiguration(configuration));

        Assert.That(exception?.Message, Does.Contain("SourceCode"));
    }

    private static IConfiguration CreateConfiguration(params (string Key, string Value)[] values)
    {
        var data = values.ToDictionary(item => item.Key, item => item.Value);

        return new ConfigurationBuilder()
            .AddInMemoryCollection(data!)
            .Build();
    }
}
