using EventsIngestion.Contracts;
using EventsIngestion.Source.Muziekladder.Parsing;
using Microsoft.Extensions.Logging;

namespace EventsIngestion.Source.Muziekladder;

/// <summary>
/// Reads events from the MLadder agenda and maps them to parsed event messages.
/// </summary>
public sealed class MLadderEventReader(
    ILogger<MLadderEventReader> logger)
{
    private const string SchemaVersion = "1.0";
    private const string SourceCode = "muziekladder";
    private const string DefaultAgendaUrl = "https://muziekladder.nl/nl/muziek/";
    private const int DelayBetweenDetailsMs = 100;

    /// <summary>
    /// Reads MLadder event pages and returns normalized parsed event messages.
    /// </summary>
    public async Task<IReadOnlyCollection<ParsedEventMessage>> ReadAsync(CancellationToken cancellationToken)
    {
        var runId = Guid.NewGuid();
        var agendaUri = new Uri(DefaultAgendaUrl);
        var agendaParser = new AgendaIndexParser();
        var detailParser = new GigDetailParser();

        using var http = CreateHttpClient();

        logger.LogInformation("Fetching MLadder agenda from {AgendaUrl}.", agendaUri);
        var agendaHtml = await GetRequiredStringAsync(http, agendaUri, cancellationToken);
        var rows = agendaParser.ExtractRows(agendaHtml, agendaUri, out var detailContainers);

        logger.LogInformation(
            "MLadder agenda returned {RowCount} event links and {DetailContainerCount} detail containers.",
            rows.Count,
            detailContainers);

        var messages = new List<ParsedEventMessage>();
        for (var i = 0; i < rows.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var row = rows[i];
            if (!Uri.TryCreate(row.Url, UriKind.Absolute, out var detailUri))
            {
                logger.LogWarning("Skipping MLadder event with invalid URL: {EventUrl}.", row.Url);
                continue;
            }

            try
            {
                logger.LogInformation(
                    "Parsing MLadder event {Current}/{Total}: {EventUrl}.",
                    i + 1,
                    rows.Count,
                    detailUri);

                var detailHtml = await GetOptionalStringAsync(http, detailUri, cancellationToken);
                if (detailHtml is null)
                    continue;

                var parsed = detailParser.Parse(detailHtml, detailUri);
                if (parsed is null)
                {
                    logger.LogWarning("MLadder event parser returned no data for {EventUrl}.", detailUri);
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(row.Title))
                    parsed.Title = row.Title;

                messages.Add(MapMessage(parsed, detailUri, runId));
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to parse MLadder event page {EventUrl}.", detailUri);
            }

            if (i < rows.Count - 1)
                await Task.Delay(DelayBetweenDetailsMs, cancellationToken);
        }

        logger.LogInformation("MLadder extraction produced {EventCount} parsed event messages.", messages.Count);
        return messages;
    }

    private static HttpClient CreateHttpClient()
    {
        var http = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
        http.DefaultRequestHeaders.TryAddWithoutValidation(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        return http;
    }

    private async Task<string> GetRequiredStringAsync(
        HttpClient http,
        Uri uri,
        CancellationToken cancellationToken)
    {
        using var response = await http.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private async Task<string?> GetOptionalStringAsync(
        HttpClient http,
        Uri uri,
        CancellationToken cancellationToken)
    {
        using var response = await http.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "Skipping MLadder event page {EventUrl}; HTTP status {StatusCode}.",
                uri,
                (int)response.StatusCode);
            return null;
        }

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    private static ParsedEventMessage MapMessage(MLadderEvent parsed, Uri detailUri, Guid runId)
    {
        var startDate = MLadderDateParser.ParseStartDate(parsed.Date);

        return new ParsedEventMessage
        {
            Metadata = new ParsedMetadata
            {
                SchemaVersion = SchemaVersion,
                MessageId = Guid.NewGuid(),
                RunId = runId,
                SourceCode = SourceCode,
                SourceEventId = GetSourceEventId(detailUri),
                ProducedAtUtc = DateTimeOffset.UtcNow
            },
            Event = new ParsedPayload
            {
                Title = parsed.Title,
                Description = parsed.Description,
                StartsAt = MLadderDateParser.MapDateTime(startDate, parsed.StartTime),
                DoorsOpenAt = MLadderDateParser.MapDateTime(startDate, parsed.DoorsTime),
                ExternalUrl = parsed.OriginalEventLink ?? detailUri.ToString(),
                Venue = MapVenue(parsed.Location),
                Images = parsed.ImageUrls
                    .Where(url => !string.IsNullOrWhiteSpace(url))
                    .Select(url => new ParsedImage { Url = url })
                    .ToArray(),
                Tickets = parsed.Tickets.Select(MapTicket).ToArray(),
                Lineup = parsed.Lineup
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Select(name => new ParsedLineupItem { Name = name })
                    .ToArray(),
                Genres = parsed.Genres
            }
        };
    }

    private static ParsedVenue? MapVenue(MLadderLocation location)
    {
        if (string.IsNullOrWhiteSpace(location.VenueName) &&
            string.IsNullOrWhiteSpace(location.Address) &&
            string.IsNullOrWhiteSpace(location.City) &&
            string.IsNullOrWhiteSpace(location.Country) &&
            location.Latitude is null &&
            location.Longitude is null)
        {
            return null;
        }

        return new ParsedVenue
        {
            Name = EmptyToNull(location.VenueName),
            Address = new ParsedAddress
            {
                AddressLine = EmptyToNull(location.Address),
                City = EmptyToNull(location.City),
                Country = EmptyToNull(location.Country),
                Latitude = location.Latitude is null ? null : Convert.ToDecimal(location.Latitude.Value),
                Longitude = location.Longitude is null ? null : Convert.ToDecimal(location.Longitude.Value)
            }
        };
    }

    private static ParsedTicket MapTicket(MLadderTicket ticket)
        => new()
        {
            Url = EmptyToNull(ticket.Url),
            PriceText = EmptyToNull(ticket.Price)
        };

    private static string GetSourceEventId(Uri detailUri)
    {
        var path = detailUri.AbsolutePath.Trim('/');
        return string.IsNullOrWhiteSpace(path) ? detailUri.ToString() : path;
    }

    private static string? EmptyToNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
