namespace EventsIngestion.Source.Muziekladder.Parsing;

internal static class UriNormalizer
{
    public static string ToAbsoluteEventPageString(Uri baseUri, string href)
    {
        if (string.IsNullOrWhiteSpace(href))
            return "";
        if (Uri.TryCreate(href, UriKind.Absolute, out var absolute) && IsHttpUri(absolute))
            return CanonicalGigUrl(absolute);
        if (Uri.TryCreate(baseUri, href, out var combined))
            return CanonicalGigUrl(combined);
        return href.Trim();
    }

    public static string ToAbsoluteResourceString(Uri baseUri, string href)
    {
        if (string.IsNullOrWhiteSpace(href))
            return "";
        if (Uri.TryCreate(href, UriKind.Absolute, out var absolute) && IsHttpUri(absolute))
            return absolute.ToString();
        if (Uri.TryCreate(baseUri, href, out var combined))
            return combined.ToString();
        return href.Trim();
    }

    private static bool IsHttpUri(Uri uri)
        => string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
           string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);

    private static string CanonicalGigUrl(Uri uri)
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
