using EventsParser.Parsing;

namespace EventsParser;

public sealed class SiteScraper(HttpClient http, ISiteProfile siteProfile)
{
    private readonly IAgendaParser _index = siteProfile.CreateAgendaParser();
    private readonly IEventParser _events = siteProfile.CreateEventParser();

    public async Task<List<EventDto>> FetchEventsAsync(
        string agendaUrl,
        int delayBetweenGigsMs = 300,
        int maxGigs = 0,
        IProgress<string>? log = null,
        CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(agendaUrl, UriKind.Absolute, out var agendaUri))
            throw new ArgumentException("Agenda URL must be absolute.", nameof(agendaUrl));

        using var req = new HttpRequestMessage(HttpMethod.Get, agendaUri);
        using var res = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        res.EnsureSuccessStatusCode();
        var html = await res.Content.ReadAsStringAsync(cancellationToken);
        var rows = _index.ExtractRows(html, agendaUri, out var detailContainers).ToList();
        log?.Report($"Found agenda rows: {rows.Count}, detail blocks: {detailContainers}.");

        var total = rows.Count;
        if (maxGigs > 0 && rows.Count > maxGigs)
        {
            rows = rows.Take(maxGigs).ToList();
            log?.Report($"Applying maxGigs={maxGigs}: processing {rows.Count} of {total} rows.");
        }

        var result = new List<EventDto>();
        for (var i = 0; i < rows.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var current = i + 1;
            var gigUrl = rows[i].Url;
            log?.Report($"[{current}/{rows.Count}] Parsing {gigUrl}");
            if (!Uri.TryCreate(gigUrl, UriKind.Absolute, out var gigUri))
            {
                log?.Report($"[{current}/{rows.Count}] Invalid event URL: {gigUrl}");
                continue;
            }

            try
            {
                using var greq = new HttpRequestMessage(HttpMethod.Get, gigUri);
                using var gres = await http.SendAsync(greq, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                if (!gres.IsSuccessStatusCode)
                {
                    log?.Report($"[{current}/{rows.Count}] Skipped (HTTP {(int)gres.StatusCode}): {gigUri}");
                    continue;
                }

                var gigHtml = await gres.Content.ReadAsStringAsync(cancellationToken);
                var dto = _events.Parse(gigHtml, gigUri);
                if (dto is null)
                {
                    log?.Report($"[{current}/{rows.Count}] Parse returned null: {gigUri}");
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(rows[i].Title))
                    dto.Title = rows[i].Title;
                result.Add(dto);
                log?.Report($"[{current}/{rows.Count}] Parsed successfully.");
            }
            catch (Exception ex)
            {
                log?.Report($"[{current}/{rows.Count}] Parse error for {gigUri}: {ex.Message}");
            }

            if (i < rows.Count - 1 && delayBetweenGigsMs > 0)
                await Task.Delay(delayBetweenGigsMs, cancellationToken);
        }

        return result;
    }
}
