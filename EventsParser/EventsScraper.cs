using EventsParser.Parsing;

namespace EventsParser;

public sealed class EventsScraper
{
    private readonly SiteScraper _inner;

    public EventsScraper(HttpClient http, MuziekladderSelectors? selectors = null)
    {
        var profile = new MuziekladderSiteProfile(selectors ?? MuziekladderSelectors.Default);
        _inner = new SiteScraper(http, profile);
    }

    public Task<List<EventDto>> FetchEventsAsync(
        string agendaUrl,
        int delayBetweenGigsMs = 300,
        int maxGigs = 0,
        IProgress<string>? log = null,
        CancellationToken cancellationToken = default)
        => _inner.FetchEventsAsync(agendaUrl, delayBetweenGigsMs, maxGigs, log, cancellationToken);
}
