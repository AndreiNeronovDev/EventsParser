using EventsIngestion.Contracts;
using EventsIngestion.Service.Abstraction;
using EventsIngestion.Source.Muziekladder;

namespace EventsIngestion.Service.Logic.Extractors;

/// <summary>
/// Muziekladder parsing handler
/// </summary>
public sealed class MLadderEventExtractor(
    MLadderEventReader reader) : IEventDataExtractor
{
    /// <inheritdoc />
    public string SourceCode => "muziekladder";

    /// <inheritdoc />
    public Task<IReadOnlyCollection<ParsedEventMessage>> ExtractAsync(CancellationToken cancellationToken)
        => reader.ReadAsync(cancellationToken);
}
