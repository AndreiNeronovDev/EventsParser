using System.Globalization;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace EventsIngestion.Source.Muziekladder.Parsing;

internal sealed class GigDetailParser
{
    private static readonly Regex EuroTicketPrice = new(
        @"(?:€|EUR)\s*\d{1,3}\s*[.,]\s*\d{2}",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex TimeRange = new(
        @"\b(\d{1,2})[:.](\d{2})(?!\.\d{4})\s*[-\u2012\u2013\u2014\u2015]\s*\d{1,2}[:.](\d{2})\b(?!\.\d{4})",
        RegexOptions.Compiled);

    private static readonly Regex SingleTime = new(
        @"\b(\d{1,2})[:.](\d{2})(?!\.\d{4})\s*(?:u\.?|uur|h)?\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex Doors = new(
        @"(?i)(deuren|doors)\s*:?\s*(\d{1,2})[:.](\d{2})(?!\.\d{4})",
        RegexOptions.Compiled);

    private static readonly Regex PriceLike = new(
        @"(€|EUR|euro|\d+[,.]\d{2})\s*(EUR|euro)?|(\d+[,.]\d{2})\s*€",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public MLadderEvent? Parse(string html, Uri eventPageUri)
    {
        var parser = new HtmlParser();
        var doc = parser.ParseDocument(html);
        var root = doc.QuerySelector(MLadderSelectors.GigPageRoot) ??
                   doc.QuerySelector(MLadderSelectors.RootScope) ??
                   doc.Body;
        if (root is null)
            return null;

        var parsed = new MLadderEvent
        {
            Title = root.QuerySelector(MLadderSelectors.Headline)?.TextContent?.Trim() ?? "",
            Date = root.QuerySelector(MLadderSelectors.DateInfo)?.TextContent?.Replace("\u00a0", " ").Trim() ?? "",
            Location = ParseLocation(root.QuerySelector(MLadderSelectors.LocationInfo), root)
        };

        var description = root.QuerySelector(MLadderSelectors.DescriptionWithItemProp);
        if (description is not null)
        {
            var plain = description.TextContent.Replace("\u00a0", " ").Trim();
            FillTimes(plain, parsed);
            parsed.Genres = MusicGenreLexicon.MatchInText($"{plain} {parsed.Title}");
            parsed.Description = Regex.Replace(plain, @"\s+", " ").Trim();
        }

        var eventLink = root.QuerySelector(MLadderSelectors.EventLink);
        if (eventLink is not null)
        {
            var url = UriNormalizer.ToAbsoluteResourceString(eventPageUri, eventLink.GetAttribute("href") ?? "");
            parsed.OriginalEventLink = string.IsNullOrEmpty(url) ? null : url;
        }

        TryFillCoordinates(root, parsed.Location);
        parsed.ImageUrls.AddRange(CollectImageUrls(root, eventPageUri));
        parsed.Tickets.AddRange(CollectTickets(root, eventPageUri));

        if (parsed.Tickets.Count == 0 && !string.IsNullOrEmpty(parsed.OriginalEventLink))
        {
            var syntheticPrice = FindEuroPriceInText(description?.TextContent) ??
                                 FindEuroPriceInText(root.TextContent);
            parsed.Tickets.Add(new MLadderTicket
            {
                Url = parsed.OriginalEventLink,
                Price = syntheticPrice
            });
        }

        parsed.Lineup.AddRange(CollectLineupNames(root));
        return parsed;
    }

    private static void FillTimes(string plainText, MLadderEvent parsed)
    {
        var textWithoutPrices = EuroTicketPrice.Replace(plainText, " ");
        var rangeMatch = TimeRange.Match(textWithoutPrices);
        if (rangeMatch.Success)
        {
            parsed.StartTime = $"{rangeMatch.Groups[1].Value}:{rangeMatch.Groups[2].Value}";
        }
        else
        {
            var singleMatch = SingleTime.Match(textWithoutPrices);
            if (singleMatch.Success)
                parsed.StartTime = $"{singleMatch.Groups[1].Value}:{singleMatch.Groups[2].Value}";
        }

        var doorsMatch = Doors.Match(textWithoutPrices);
        if (doorsMatch.Success)
            parsed.DoorsTime = $"{doorsMatch.Groups[2].Value}:{doorsMatch.Groups[3].Value}";
    }

    private static string? FindEuroPriceInText(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return null;

        var match = EuroTicketPrice.Match(text.Replace("\u00a0", " "));
        return match.Success ? NormalizeWhitespace(match.Value) : null;
    }

    private MLadderLocation ParseLocation(IElement? locationInfo, IElement root)
    {
        var location = new MLadderLocation();

        var city = locationInfo?.QuerySelector(MLadderSelectors.CityName);
        if (city is not null)
            location.City = city.TextContent.Trim();

        var country = locationInfo?.QuerySelector(MLadderSelectors.CountryName);
        if (country is not null)
            location.Country = country.TextContent.Trim();

        var block = root.QuerySelector(MLadderSelectors.LocationBlock);
        if (block is not null)
        {
            var venueAnchor = block.QuerySelector("h3 a[href], h3 a");
            if (venueAnchor is not null)
                location.VenueName = venueAnchor.TextContent.Trim();

            var list = block.QuerySelector("ul.list-unstyled");
            if (list is not null)
            {
                var lines = list.QuerySelectorAll("li")
                    .Select(item => item.TextContent.Replace("\u00a0", " ").Trim())
                    .Where(text => text.Length > 0);
                location.Address = string.Join(", ", lines);
            }
        }

        if (string.IsNullOrEmpty(location.VenueName) && locationInfo is not null)
        {
            var firstLink = locationInfo.QuerySelector("a[href]");
            if (firstLink is not null)
                location.VenueName = firstLink.TextContent.Trim();
        }

        return location;
    }

    private static void TryFillCoordinates(IElement root, MLadderLocation location)
    {
        foreach (var element in root.QuerySelectorAll("a[href*='google'], iframe[src*='google'], iframe[src*='openstreetmap']"))
        {
            var href = element.GetAttribute("href") ?? element.GetAttribute("src") ?? "";
            var match = Regex.Match(href, @"(-?\d{1,2}\.\d+)\s*,\s*(-?\d{1,3}\.\d+)");
            if (!match.Success)
                continue;

            if (double.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude) &&
                double.TryParse(match.Groups[2].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude) &&
                Math.Abs(latitude) <= 90 &&
                Math.Abs(longitude) <= 180)
            {
                location.Latitude = latitude;
                location.Longitude = longitude;
                return;
            }
        }
    }

    private static List<string> CollectImageUrls(IElement root, Uri baseUri)
    {
        var urls = new List<string>();
        foreach (var image in root.QuerySelectorAll("img[src]"))
        {
            var src = image.GetAttribute("src") ?? "";
            if (string.IsNullOrWhiteSpace(src) || src.Contains("data:", StringComparison.OrdinalIgnoreCase))
                continue;

            var url = UriNormalizer.ToAbsoluteResourceString(baseUri, src);
            if (!string.IsNullOrEmpty(url) && !urls.Contains(url, StringComparer.OrdinalIgnoreCase))
                urls.Add(url);
        }

        return urls;
    }

    private static List<MLadderTicket> CollectTickets(IElement root, Uri baseUri)
    {
        var tickets = new List<MLadderTicket>();
        foreach (var anchor in root.QuerySelectorAll("a[href]"))
        {
            var href = anchor.GetAttribute("href") ?? "";
            if (!LooksLikeTicketUrl(href))
                continue;

            var url = UriNormalizer.ToAbsoluteResourceString(baseUri, href);
            if (tickets.Any(ticket => string.Equals(ticket.Url, url, StringComparison.OrdinalIgnoreCase)))
                continue;

            tickets.Add(new MLadderTicket
            {
                Url = url,
                Price = ExtractPriceNearAnchor(anchor)
            });
        }

        return tickets;
    }

    private static bool LooksLikeTicketUrl(string href)
        => href.Contains("ticket", StringComparison.OrdinalIgnoreCase) ||
           href.Contains("tix", StringComparison.OrdinalIgnoreCase) ||
           href.Contains("eventim", StringComparison.OrdinalIgnoreCase) ||
           href.Contains("ticketmaster", StringComparison.OrdinalIgnoreCase);

    private static string? ExtractPriceNearAnchor(IElement anchor)
    {
        var text = anchor.TextContent.Replace("\u00a0", " ").Trim();
        var euroMatch = EuroTicketPrice.Match(text);
        if (euroMatch.Success)
            return NormalizeWhitespace(euroMatch.Value);

        if (PriceLike.IsMatch(text))
            return text.Length > 0 ? text : null;

        var parentText = anchor.ParentElement?.TextContent.Replace("\u00a0", " ").Trim();
        if (string.IsNullOrEmpty(parentText))
            return null;

        var parentEuroMatch = EuroTicketPrice.Match(parentText);
        if (parentEuroMatch.Success)
            return NormalizeWhitespace(parentEuroMatch.Value);

        var parentPriceMatch = PriceLike.Match(parentText);
        return parentPriceMatch.Success ? parentPriceMatch.Value.Trim() : null;
    }

    private static List<string> CollectLineupNames(IElement root)
    {
        var lineup = new List<string>();
        var node = root.QuerySelector(".lineup, .line-up");
        if (node is null)
            return lineup;

        foreach (var item in node.QuerySelectorAll("li"))
        {
            var name = item.TextContent.Replace("\u00a0", " ").Trim();
            if (name.Length > 0)
                lineup.Add(name);
        }

        return lineup;
    }

    private static string NormalizeWhitespace(string text)
        => Regex.Replace(text.Trim(), @"\s+", " ");
}
