using EventsIngestion.Contracts;
using EventsIngestion.Service.Abstraction;

namespace EventsIngestion.Service.Logic;

/// <summary>
/// Dummy extractor used to verify source-code dispatch without real parsing logic.
/// </summary>
public sealed class FirstDummyEventDataExtractor : IEventDataExtractor
{
    /// <inheritdoc />
    public string SourceCode => "dummy-one";

    /// <inheritdoc />
    public Task<IReadOnlyCollection<ParsedEventMessage>> ExtractAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Dummy extractor one was selected and executed.");
        return Task.FromResult<IReadOnlyCollection<ParsedEventMessage>>([]);
    }
}
