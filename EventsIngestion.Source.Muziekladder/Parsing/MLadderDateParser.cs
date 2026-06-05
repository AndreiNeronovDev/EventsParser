using System.Globalization;

namespace EventsIngestion.Source.Muziekladder.Parsing;

internal static class MLadderDateParser
{
    private const string SourceTimeZoneId = "Europe/Amsterdam";
    private static readonly CultureInfo SourceCulture = CultureInfo.GetCultureInfo("nl-NL");

    public static DateTime? ParseStartDate(string? dateText)
    {
        if (string.IsNullOrWhiteSpace(dateText))
            return null;

        var normalized = dateText.Replace("\u00a0", " ").Trim();
        var formats = new[] { "dddd d MMMM yyyy", "d MMMM yyyy" };

        return DateTime.TryParseExact(
            normalized,
            formats,
            SourceCulture,
            DateTimeStyles.AllowWhiteSpaces,
            out var date)
            ? date.Date
            : null;
    }

    public static DateTimeOffset? MapStartsAt(DateTime? startDate, string? startTimeText)
        => MapDateTime(startDate, startTimeText);

    public static DateTimeOffset? MapDateTime(DateTime? date, string? timeText)
    {
        if (date is null || string.IsNullOrWhiteSpace(timeText))
            return null;

        var formats = new[] { "H:mm", "HH:mm", "H.mm", "HH.mm" };
        if (!DateTime.TryParseExact(
                timeText.Trim(),
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var time))
        {
            return null;
        }

        if (!TimeZoneInfo.TryFindSystemTimeZoneById(SourceTimeZoneId, out var timeZone))
            return null;

        var localStart = date.Value.Date.Add(time.TimeOfDay);
        return new DateTimeOffset(localStart, timeZone.GetUtcOffset(localStart));
    }
}
