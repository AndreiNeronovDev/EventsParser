namespace EventsParser.Parsing;

internal static class UriNormalizer
{
    public static string ToAbsoluteString(Uri baseUri, string href)
    {
        if (string.IsNullOrWhiteSpace(href))
            return "";
        if (Uri.TryCreate(href, UriKind.Absolute, out var abs))
            return CanonicalGigUrl(abs);
        if (Uri.TryCreate(baseUri, href, out var combined))
            return CanonicalGigUrl(combined);
        return href.Trim();
    }

    public static string CanonicalGigUrl(Uri uri)
    {
        var path = uri.AbsolutePath;
        if (path.StartsWith("/gig/", StringComparison.Ordinal) &&
            uri.Host.Contains("muziekladder", StringComparison.OrdinalIgnoreCase))
        {
            var builder = new UriBuilder(uri) { Path = "/nl" + path };
            return builder.Uri.GetLeftPart(UriPartial.Path);
        }

        return uri.GetLeftPart(UriPartial.Path);
    }
}
