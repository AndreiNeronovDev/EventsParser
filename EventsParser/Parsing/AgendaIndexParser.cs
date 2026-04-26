using AngleSharp.Html.Parser;

namespace EventsParser.Parsing;

public sealed class AgendaIndexParser(MuziekladderSelectors selectors)
    : IAgendaParser
{
    private readonly MuziekladderSelectors _selectors = selectors;

    public IReadOnlyList<AgendaListRow> ExtractRows(string html, Uri pageUri, out int detailContainers)
    {
        var parser = new HtmlParser();
        var doc = parser.ParseDocument(html);
        var root = doc.QuerySelector(_selectors.RootScope) ?? doc.Body;
        if (root is null)
        {
            detailContainers = 0;
            return [];
        }

        detailContainers = root.QuerySelectorAll(_selectors.DetailContainer).Length;

        var ordered = new List<AgendaListRow>();

        foreach (var row in root.QuerySelectorAll(_selectors.GigRow))
        {
            var a = row.QuerySelector(".first-col a[href]") ??
                    row.QuerySelector("a[itemprop='url'][href]") ??
                    row.QuerySelector("a[href]");
            if (a is null)
                continue;
            var href = a.GetAttribute("href") ?? "";
            if (!href.Contains(_selectors.GigAnchorHrefContains, StringComparison.OrdinalIgnoreCase))
                continue;
            var abs = UriNormalizer.ToAbsoluteString(pageUri, href);
            if (string.IsNullOrEmpty(abs))
                continue;
            var title = a.TextContent?.Trim() ?? "";
            ordered.Add(new AgendaListRow(abs, title));

        }

        if (ordered.Count > 0)
            return ordered;

        foreach (var a in root.QuerySelectorAll("a[href]"))
        {
            var href = a.GetAttribute("href") ?? "";
            if (!href.Contains(_selectors.GigAnchorHrefContains, StringComparison.OrdinalIgnoreCase))
                continue;
            var abs = UriNormalizer.ToAbsoluteString(pageUri, href);
            if (string.IsNullOrEmpty(abs))
                continue;
            ordered.Add(new AgendaListRow(abs, a.TextContent?.Trim() ?? ""));
        }

        return ordered;
    }

}
