using EventsIngestion.Contracts;
using EventsIngestion.Service.Abstraction;
using EventsIngestion.Service.Logic;
using Microsoft.Extensions.Logging;
using Moq;

namespace EventsIngestion.Service.Tests;

public sealed class EventIngestionServiceTests
{
    [Test]
    public async Task RunAsync_ExtractsEventsAndPublishesThem()
    {
        var messages = new[] { CreateMessage("event-1"), CreateMessage("event-2") };
        var extractor = CreateExtractor("muziekladder", messages);
        var publisher = new Mock<IMessagePublisher>();
        publisher
            .Setup(item => item.PublishBatchAsync(messages, It.IsAny<CancellationToken>()))
            .ReturnsAsync(messages.Length);

        var service = CreateService(extractor, publisher.Object);

        var result = await service.RunAsync("muziekladder", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.SourceCode, Is.EqualTo("muziekladder"));
            Assert.That(result.ExtractedCount, Is.EqualTo(messages.Length));
            Assert.That(result.PublishedCount, Is.EqualTo(messages.Length));
        });

        publisher.Verify(
            item => item.PublishBatchAsync(messages, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task RunAsync_ReturnsPublishedCountFromPublisher()
    {
        var messages = new[] { CreateMessage("event-1"), CreateMessage("event-2"), CreateMessage("event-3") };
        var extractor = CreateExtractor("muziekladder", messages);
        var publisher = new Mock<IMessagePublisher>();
        publisher
            .Setup(item => item.PublishBatchAsync(messages, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var service = CreateService(extractor, publisher.Object);

        var result = await service.RunAsync("muziekladder", CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExtractedCount, Is.EqualTo(3));
            Assert.That(result.PublishedCount, Is.EqualTo(2));
        });
    }

    [Test]
    public void RunAsync_Throws_WhenSourceCodeIsUnknown()
    {
        var extractor = CreateExtractor("muziekladder", Array.Empty<ParsedEventMessage>());
        var service = CreateService(extractor, Mock.Of<IMessagePublisher>());

        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RunAsync("ticketmaster", CancellationToken.None));

        Assert.That(exception?.Message, Does.Contain("ticketmaster"));
    }

    private static EventIngestionService CreateService(
        IEventDataExtractor extractor,
        IMessagePublisher publisher)
    {
        var registry = new EventExtractorsRegistry(
            [extractor],
            Mock.Of<ILogger<EventExtractorsRegistry>>());

        return new EventIngestionService(
            registry,
            publisher,
            Mock.Of<ILogger<EventIngestionService>>());
    }

    private static IEventDataExtractor CreateExtractor(
        string sourceCode,
        IReadOnlyCollection<ParsedEventMessage> messages)
    {
        var extractor = new Mock<IEventDataExtractor>();
        extractor.SetupGet(item => item.SourceCode).Returns(sourceCode);
        extractor
            .Setup(item => item.ExtractAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(messages);

        return extractor.Object;
    }

    private static ParsedEventMessage CreateMessage(string sourceEventId)
        => new()
        {
            Metadata = new ParsedMetadata
            {
                SchemaVersion = "1.0",
                MessageId = Guid.NewGuid(),
                RunId = Guid.NewGuid(),
                SourceCode = "muziekladder",
                SourceEventId = sourceEventId,
                ProducedAtUtc = DateTimeOffset.UtcNow
            },
            Event = new ParsedPayload
            {
                Title = sourceEventId
            }
        };
}
