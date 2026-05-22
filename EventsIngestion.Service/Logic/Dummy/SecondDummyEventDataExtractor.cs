using EventsIngestion.Contracts;
using EventsIngestion.Service.Abstraction;

namespace EventsIngestion.Service.Logic;

/// <summary>
/// Dummy extractor used to verify another source-code dispatch path.
/// </summary>
public sealed class SecondDummyEventDataExtractor : IEventDataExtractor
{
    /// <inheritdoc />
    public string SourceCode => "dummy-two";

    /// <inheritdoc />
    public Task<IReadOnlyCollection<ParsedEventMessage>> ExtractAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Dummy extractor two was selected and executed.");
        return Task.FromResult<IReadOnlyCollection<ParsedEventMessage>>([]);
    }
}
