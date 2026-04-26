using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EventsParser;

internal static class ParseRun
{
    public static async Task RunOnceAsync(
        SiteScraper scraper,
        string agendaUrl,
        int delayMs,
        int maxGigs,
        string outputPath,
        IProgress<string> progress,
        CancellationToken cancellationToken)
    {
        var events = await scraper.FetchEventsAsync(agendaUrl, delayMs, maxGigs, progress, cancellationToken);
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };
        var json = JsonSerializer.Serialize(events, options);
        var full = Path.GetFullPath(outputPath);
        var dir = Path.GetDirectoryName(full);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        var temp = full + ".tmp." + Guid.NewGuid().ToString("N");
        await File.WriteAllTextAsync(temp, json, cancellationToken);
        File.Move(temp, full, overwrite: true);
        Console.WriteLine($"Done: {events.Count} events written to {full}");
    }
}
