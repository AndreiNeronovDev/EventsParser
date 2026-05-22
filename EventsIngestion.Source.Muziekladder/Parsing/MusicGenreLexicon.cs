using System.Text.RegularExpressions;

namespace EventsIngestion.Source.Muziekladder.Parsing;

internal static class MusicGenreLexicon
{
    private static readonly string[] RawTerms =
    [
        "acid house", "acid jazz", "afrobeat", "alternative", "alternative rock", "americana",
        "ambient", "black metal", "blues", "blues rock", "breakbeat", "classical",
        "country", "dance", "death metal", "deep house", "disco", "doom metal",
        "drum and bass", "drum & bass", "dub", "dubstep", "electro", "electronic",
        "elektronisch", "experimental", "folk", "folk rock", "funk", "garage rock",
        "gospel", "hard rock", "hardcore", "heavy metal", "hip hop", "hip-hop",
        "house", "indie", "indie pop", "indie rock", "industrial", "jazz",
        "klassiek", "latin", "metal", "metalcore", "minimal techno", "new wave",
        "opera", "pop", "pop punk", "pop rock", "post-punk", "post-rock",
        "prog rock", "punk", "punk rock", "rap", "reggae", "rock",
        "rock & roll", "rockabilly", "singer songwriter", "singer-songwriter",
        "ska", "soul", "stoner rock", "synthpop", "techno", "trance",
        "tribute", "wereldmuziek", "world music", "acoustic", "akoestisch"
    ];

    private static readonly string[] TermsByLengthDesc = RawTerms
        .Select(term => term.Trim())
        .Where(term => term.Length > 0)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderByDescending(term => term.Length)
        .ThenBy(term => term, StringComparer.OrdinalIgnoreCase)
        .ToArray();

    public static List<string> MatchInText(string plainAndTitle, int maxCount = 8)
    {
        var normalized = NormalizeForMatch(plainAndTitle);
        if (normalized.Length == 0)
            return [];

        var chosen = new List<string>();
        foreach (var term in TermsByLengthDesc)
        {
            var lowerTerm = term.ToLowerInvariant();
            if (!ContainsAsPhrase(normalized, lowerTerm))
                continue;

            chosen.RemoveAll(existing =>
                term.Length > existing.Length &&
                term.Contains(existing, StringComparison.OrdinalIgnoreCase));

            if (chosen.Any(existing =>
                    existing.Contains(term, StringComparison.OrdinalIgnoreCase) &&
                    existing.Length >= term.Length))
            {
                continue;
            }

            chosen.Add(term);
        }

        chosen.Sort((left, right) =>
            FirstMatchIndex(normalized, left.ToLowerInvariant())
                .CompareTo(FirstMatchIndex(normalized, right.ToLowerInvariant())));

        return chosen.Take(maxCount).ToList();
    }

    private static string NormalizeForMatch(string value)
    {
        var normalized = value.Replace("\u00a0", " ", StringComparison.Ordinal).ToLowerInvariant();
        normalized = Regex.Replace(normalized, @"[^\p{L}\p{N}\s&/'+-]", " ");
        return Regex.Replace(normalized, @"\s+", " ").Trim();
    }

    private static bool ContainsAsPhrase(string normalizedLower, string termLower)
    {
        var pattern = BuildPhrasePattern(termLower);
        return Regex.IsMatch(normalizedLower, pattern, RegexOptions.CultureInvariant);
    }

    private static int FirstMatchIndex(string normalizedLower, string termLower)
    {
        var match = Regex.Match(normalizedLower, BuildPhrasePattern(termLower), RegexOptions.CultureInvariant);
        return match.Success ? match.Index : int.MaxValue;
    }

    private static string BuildPhrasePattern(string termLower)
    {
        var parts = termLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return "(?<![\\p{L}\\p{M}0-9])" +
               string.Join(@"\s+", parts.Select(Regex.Escape)) +
               @"(?![\p{L}\p{M}0-9])";
    }
}
