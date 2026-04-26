using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace EventsParser.Parsing;

public sealed class GigDetailParser(MuziekladderSelectors selectors)
    : IEventParser
{
    private static readonly Regex EuroPriceForMask = new(
        @"(?:[€\u20AC]|EUR)\s*\d{1,2}\s*[.,]\s*\d{2}",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex EuroTicketPrice = new(
        @"(?:[€\u20AC]|EUR)\s*\d{1,2}\s*[.,]\s*\d{2}",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex TimeRange = new(
        @"\b(\d{1,2})[:.](\d{2})(?!\.\d{4})\s*[–\-]\s*\d{1,2}[:.](\d{2})\b(?!\.\d{4})",
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

    private readonly MuziekladderSelectors _selectors = selectors;

    public EventDto? Parse(string html, Uri gigPageUri)
    {
        var parser = new HtmlParser();
        var doc = parser.ParseDocument(html);
        var root = doc.QuerySelector(_selectors.GigPageRoot) ?? doc.QuerySelector(_selectors.RootScope) ?? doc.Body;
        if (root is null)
            return null;

        var ev = new EventDto();

        var h = root.QuerySelector(_selectors.Headline);
        ev.Title = h?.TextContent?.Trim() ?? "";

        var dateEl = root.QuerySelector(_selectors.DateInfo);
        ev.Date = dateEl?.TextContent?.Replace("\u00a0", " ").Trim() ?? "";

        var locInfo = root.QuerySelector(_selectors.LocationInfo);
        ev.Location = ParseLocation(locInfo, root, gigPageUri);

        var desc = root.QuerySelector(_selectors.DescriptionWithItemProp);
        if (desc is not null)
        {
            var plain = desc.TextContent.Replace("\u00a0", " ").Trim();
            var forTime = MaskEuroPricesForTimeScan(plain);
            var m = TimeRange.Match(forTime);
            if (m.Success)
                ev.StartTime = $"{m.Groups[1].Value}:{m.Groups[2].Value}";
            else
            {
                var sm = SingleTime.Match(forTime);
                if (sm.Success)
                    ev.StartTime = $"{sm.Groups[1].Value}:{sm.Groups[2].Value}";
            }

            var dm = Doors.Match(forTime);
            if (dm.Success)
                ev.DoorsTime = $"{dm.Groups[2].Value}:{dm.Groups[3].Value}";

            ev.Genres = MusicGenreLexicon.MatchInText($"{plain} {ev.Title}");
            ev.Description = Regex.Replace(plain, @"\s+", " ").Trim();
        }

        var ext = root.QuerySelector(_selectors.EventLink);
        if (ext is not null)
        {
            var u = UriNormalizer.ToAbsoluteString(gigPageUri, ext.GetAttribute("href") ?? "");
            ev.OriginalEventLink = string.IsNullOrEmpty(u) ? null : u;
        }

        TryFillCoordinates(root, ev.Location);

        foreach (var url in CollectImageUrls(root, gigPageUri))
            ev.ImageUrls.Add(url);

        ev.Tickets.AddRange(CollectTickets(root, gigPageUri));
        if (ev.Tickets.Count == 0 && !string.IsNullOrEmpty(ev.OriginalEventLink))
        {
            var syntheticPrice = FindEuroPriceInText(desc?.TextContent) ??
                                 FindEuroPriceInText(root.TextContent);
            ev.Tickets.Add(new TicketDto { Url = ev.OriginalEventLink!, Price = syntheticPrice });
        }

        ev.Lineup.AddRange(CollectLineupNames(root));

        return ev;
    }

    private static string MaskEuroPricesForTimeScan(string text) =>
        EuroPriceForMask.Replace(text, " ");

    private static string? FindEuroPriceInText(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return null;
        var normalized = text.Replace("\u00a0", " ");
        var m = EuroTicketPrice.Match(normalized);
        return m.Success ? NormalizePriceWhitespace(m.Value) : null;
    }

    private static string NormalizePriceWhitespace(string s) =>
        Regex.Replace(s.Trim(), @"\s+", " ");

    private LocationDto ParseLocation(IElement? locInfo, IElement root, Uri baseUri)
    {
        var loc = new LocationDto();

        var city = locInfo?.QuerySelector(_selectors.CityName);
        if (city is not null)
            loc.City = city.TextContent.Trim();

        var country = locInfo?.QuerySelector(_selectors.CountryName);
        if (country is not null)
            loc.Country = country.TextContent.Trim();

        var block = root.QuerySelector(_selectors.LocationBlock);
        if (block is not null)
        {
            var venueA = block.QuerySelector("h3 a[href], h3 a");
            if (venueA is not null)
                loc.VenueName = venueA.TextContent.Trim();

            var ul = block.QuerySelector("ul.list-unstyled");
            if (ul is not null)
            {
                var lines = ul.QuerySelectorAll("li")
                    .Select(li => li.TextContent.Replace("\u00a0", " ").Trim())
                    .Where(t => t.Length > 0)
                    .ToList();
                loc.Address = string.Join(", ", lines);
            }
        }

        if (string.IsNullOrEmpty(loc.VenueName) && locInfo is not null)
        {
            var firstLink = locInfo.QuerySelector("a[href]");
            if (firstLink is not null)
                loc.VenueName = firstLink.TextContent.Trim();
        }

        return loc;
    }

    private static void TryFillCoordinates(IElement root, LocationDto loc)
    {
        foreach (var el in root.QuerySelectorAll("a[href*='google'], iframe[src*='google'], iframe[src*='openstreetmap']"))
        {
            var href = el.GetAttribute("href") ?? el.GetAttribute("src") ?? "";
            var m = Regex.Match(href, @"(-?\d{1,2}\.\d+)\s*,\s*(-?\d{1,3}\.\d+)");
            if (!m.Success)
                continue;
            if (double.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var lat) &&
                double.TryParse(m.Groups[2].Value, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var lon) &&
                Math.Abs(lat) <= 90 && Math.Abs(lon) <= 180)
            {
                loc.Latitude = lat;
                loc.Longitude = lon;
                return;
            }
        }
    }

    private static List<string> CollectImageUrls(IElement root, Uri baseUri)
    {
        var list = new List<string>();
        foreach (var img in root.QuerySelectorAll("img[src]"))
        {
            var src = img.GetAttribute("src") ?? "";
            if (string.IsNullOrWhiteSpace(src) || src.Contains("data:", StringComparison.OrdinalIgnoreCase))
                continue;
            var url = UriNormalizer.ToAbsoluteString(baseUri, src);
            if (list.Contains(url))
                continue;
            list.Add(url);
        }

        return list;
    }

    private static List<TicketDto> CollectTickets(IElement root, Uri baseUri)
    {
        var list = new List<TicketDto>();
        foreach (var a in root.QuerySelectorAll("a[href]"))
        {
            var href = a.GetAttribute("href") ?? "";
            if (!LooksLikeTicketUrl(href))
                continue;
            var abs = UriNormalizer.ToAbsoluteString(baseUri, href);
            if (list.Any(t => t.Url == abs))
                continue;
            var price = ExtractPriceNearAnchor(a);
            list.Add(new TicketDto { Url = abs, Price = price });
        }

        return list;
    }

    private static bool LooksLikeTicketUrl(string href) =>
        href.Contains("ticket", StringComparison.OrdinalIgnoreCase) ||
        href.Contains("tix", StringComparison.OrdinalIgnoreCase) ||
        href.Contains("eventim", StringComparison.OrdinalIgnoreCase) ||
        href.Contains("ticketmaster", StringComparison.OrdinalIgnoreCase);

    private static string? ExtractPriceNearAnchor(IElement a)
    {
        var t = a.TextContent.Replace("\u00a0", " ").Trim();
        var euro = EuroTicketPrice.Match(t);
        if (euro.Success)
            return NormalizePriceWhitespace(euro.Value);
        if (PriceLike.IsMatch(t))
            return t.Length > 0 ? t : null;
        var parent = a.ParentElement;
        if (parent is not null)
        {
            var pt = parent.TextContent.Replace("\u00a0", " ").Trim();
            var em = EuroTicketPrice.Match(pt);
            if (em.Success)
                return NormalizePriceWhitespace(em.Value);
            var m = PriceLike.Match(pt);
            if (m.Success)
                return m.Value.Trim();
        }

        return null;
    }

    private static List<string> CollectLineupNames(IElement root)
    {
        var list = new List<string>();
        var node = root.QuerySelector(".lineup, .line-up");
        if (node is null)
            return list;
        foreach (var li in node.QuerySelectorAll("li"))
        {
            var name = li.TextContent.Replace("\u00a0", " ").Trim();
            if (name.Length > 0)
                list.Add(name);
        }

        return list;
    }

}
