using AngleSharp.Html.Parser;

namespace EventsIngestion.Source.Muziekladder.Parsing;

internal sealed class AgendaIndexParser
{
    public IReadOnlyList<AgendaListRow> ExtractRows(string html, Uri pageUri, out int detailContainers)
    {
        var parser = new HtmlParser();
        var doc = parser.ParseDocument(html);
        var root = doc.QuerySelector(MLadderSelectors.RootScope) ?? doc.Body;
        if (root is null)
        {
            detailContainers = 0;
            return [];
        }

        detailContainers = root.QuerySelectorAll(MLadderSelectors.DetailContainer).Length;
        var ordered = new List<AgendaListRow>();

        foreach (var row in root.QuerySelectorAll(MLadderSelectors.GigRow))
        {
            var anchor = row.QuerySelector(".first-col a[href]") ??
                         row.QuerySelector("a[itemprop='url'][href]") ??
                         row.QuerySelector("a[href]");
            if (anchor is null)
                continue;

            var href = anchor.GetAttribute("href") ?? "";
            if (!href.Contains(MLadderSelectors.GigAnchorHrefContains, StringComparison.OrdinalIgnoreCase))
                continue;

            var absolute = UriNormalizer.ToAbsoluteEventPageString(pageUri, href);
            if (string.IsNullOrEmpty(absolute))
                continue;

            ordered.Add(new AgendaListRow(absolute, anchor.TextContent?.Trim() ?? ""));
        }

        if (ordered.Count > 0)
            return ordered.DistinctBy(row => row.Url, StringComparer.OrdinalIgnoreCase).ToArray();

        foreach (var anchor in root.QuerySelectorAll("a[href]"))
        {
            var href = anchor.GetAttribute("href") ?? "";
            if (!href.Contains(MLadderSelectors.GigAnchorHrefContains, StringComparison.OrdinalIgnoreCase))
                continue;

            var absolute = UriNormalizer.ToAbsoluteEventPageString(pageUri, href);
            if (!string.IsNullOrEmpty(absolute))
                ordered.Add(new AgendaListRow(absolute, anchor.TextContent?.Trim() ?? ""));
        }

        return ordered.DistinctBy(row => row.Url, StringComparer.OrdinalIgnoreCase).ToArray();
    }
}
