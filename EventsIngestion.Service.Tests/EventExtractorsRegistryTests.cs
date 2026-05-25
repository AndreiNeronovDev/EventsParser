using EventsIngestion.Service.Abstraction;
using EventsIngestion.Service.Logic;
using Microsoft.Extensions.Logging;
using Moq;

namespace EventsIngestion.Service.Tests;

public sealed class EventExtractorsRegistryTests
{
    [Test]
    public void GetExtractor_ReturnsExtractor_WhenSourceCodeIsRegistered()
    {
        var extractor = CreateExtractor("muziekladder");
        var registry = CreateRegistry(extractor);

        var result = registry.GetExtractor("muziekladder");

        Assert.That(result, Is.SameAs(extractor));
    }

    [Test]
    public void GetExtractor_ReturnsExtractor_WhenSourceCodeCasingDiffers()
    {
        var extractor = CreateExtractor("muziekladder");
        var registry = CreateRegistry(extractor);

        var result = registry.GetExtractor("MUZIEKLADDER");

        Assert.That(result, Is.SameAs(extractor));
    }

    [Test]
    public void GetExtractor_Throws_WhenSourceCodeIsUnknown()
    {
        var registry = CreateRegistry(CreateExtractor("muziekladder"));

        var exception = Assert.Throws<InvalidOperationException>(() => registry.GetExtractor("ticketmaster"));

        Assert.That(exception?.Message, Does.Contain("ticketmaster"));
    }

    private static EventExtractorsRegistry CreateRegistry(params IEventDataExtractor[] extractors)
        => new(extractors, Mock.Of<ILogger<EventExtractorsRegistry>>());

    private static IEventDataExtractor CreateExtractor(string sourceCode)
    {
        var extractor = new Mock<IEventDataExtractor>();
        extractor.SetupGet(item => item.SourceCode).Returns(sourceCode);
        extractor
            .Setup(item => item.ExtractAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        return extractor.Object;
    }
}
