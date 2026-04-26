using System.Text.RegularExpressions;

namespace EventsParser.Parsing;

internal static class MusicGenreLexicon
{
    private static readonly string[] RawTerms =
    [
        "acid house", "acid jazz", "afro house", "afrobeat", "alternative", "alternative rock", "americana",
        "ambient", "arena rock", "art rock", "avant-garde", "bachata", "barok", "baroque", "balkan",
        "bebop", "big band", "black metal", "blues", "blues rock", "bluegrass", "bollywood", "bossa nova",
        "braziliaans", "breakbeat", "breakcore", "britpop", "calypso", "cabaret", "celtic", "celtic rock",
        "chamber music", "chanson", "children's music", "choir", "choral", "classic rock", "classical",
        "club", "comedy", "contemporary classical", "country", "country rock", "crossover", "dance",
        "dancehall", "dark ambient", "dark wave", "death metal", "deathcore", "deep house", "disco",
        "dj-set", "dj set", "dnb", "doom metal", "downtempo", "drill", "drum and bass", "drum & bass",
        "dub", "dub techno", "dubstep", "easy listening", "edm", "electro", "electro swing", "electronic",
        "elektronisch", "elektronische muziek", "emo", "ethio jazz", "experimental", "fado", "flamenco",
        "folk", "folk metal", "folk rock", "funk", "fusion", "future bass", "future house", "garage",
        "garage rock", "glam rock", "goa", "gospel", "gothic", "gothic metal", "gregorian", "grime",
        "groove", "grunge", "gypsy jazz", "hard rock", "hard techno", "hardcore", "hardcore punk",
        "hardstyle", "heavy metal", "highlife", "hip hop", "hip-hop", "house", "hyperpop", "idm",
        "impro", "improvisation", "improvisatie", "indie", "indie folk", "indie pop", "indie rock",
        "industrial", "irish folk", "italo disco", "jazz", "jazz fusion", "jazz rock", "jazzrock",
        "jungle", "kamerconcert", "kamerkonzert", "kamermuziek", "kleinkunst", "klezmer", "klassiek",
        "klassieke muziek", "koor", "koormuziek", "latin", "latin jazz", "levenslied", "liquid funk",
        "lo-fi", "lofi", "lounge", "manouche", "math rock", "melodic house", "metal", "metalcore",
        "minimal", "minimal techno", "moombahton", "musical", "muziek overig", "muziektheater", "neo soul",
        "neosoul", "new age", "new wave", "nederbeat", "nederpop", "noise", "noise rock", "nu metal",
        "opera", "operette", "oratorium", "orchestral", "piano", "polka", "pop", "pop punk", "pop rock",
        "post-hardcore", "post-metal", "post-punk", "post-rock", "power metal", "prog", "prog metal",
        "prog rock", "progressive house", "progressive metal", "progressive rock", "psy trance",
        "psychedelic rock", "punk", "punk rock", "r&b", "r'n'b", "ragtime", "rap", "reggae",
        "renaissance", "rnb", "rock", "rock & roll", "rock and roll", "rockabilly", "romantiek",
        "roots", "roots reggae", "samba", "schlager", "schauspiel", "singer songwriter", "singer-songwriter",
        "ska", "ska punk", "sludge metal", "smooth jazz", "smartlap", "smartlappen", "soul", "soundtrack",
        "speed metal", "spoken word", "stoner rock", "street punk", "surf rock", "swing", "symfonisch",
        "symphonic metal", "symphonic rock", "symphony", "synth-pop", "synthpop", "synthwave", "tech house",
        "techno", "thrash metal", "toneel", "trance", "trap", "tribute", "trip hop", "trip-hop",
        "tropical house", "uk garage", "urban", "vocal", "volksmuziek", "wereldmuziek", "witch house",
        "world music", "zouk", "zydeco", "à capella", "a capella", "acapella", "akoestisch", "acoustic",
        "dansvoorstelling", "dans", "ballet", "contemporary dance", "elektro", "jump up",
        "speedcore", "uplifting trance", "vaporwave", "folkpunk", "klezmerrock"
    ];

    private static readonly string[] TermsByLengthDesc = RawTerms
        .Select(t => t.Trim())
        .Where(t => t.Length > 0)
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .OrderByDescending(t => t.Length)
        .ThenBy(t => t, StringComparer.OrdinalIgnoreCase)
        .ToArray();

    internal static List<string> MatchInText(string plainAndTitle, int maxCount = 8)
    {
        var norm = NormalizeForMatch(plainAndTitle);
        if (norm.Length == 0)
            return [];

        var chosen = new List<string>();
        foreach (var term in TermsByLengthDesc)
        {
            var tl = term.ToLowerInvariant();
            if (!ContainsAsPhrase(norm, tl))
                continue;

            chosen.RemoveAll(s =>
                term.Length > s.Length &&
                term.Contains(s, StringComparison.OrdinalIgnoreCase));

            if (chosen.Any(s =>
                    s.Contains(term, StringComparison.OrdinalIgnoreCase) &&
                    s.Length >= term.Length))
                continue;

            chosen.Add(term);
        }

        chosen.Sort((a, b) =>
            FirstMatchIndex(norm, a.ToLowerInvariant()).CompareTo(FirstMatchIndex(norm, b.ToLowerInvariant())));

        return chosen.Take(maxCount).ToList();
    }

    private static string NormalizeForMatch(string s)
    {
        var t = s.Replace("\u00a0", " ", StringComparison.Ordinal).ToLowerInvariant();
        t = Regex.Replace(t, @"[^\p{L}\p{N}\s&/'+-]", " ");
        return Regex.Replace(t, @"\s+", " ").Trim();
    }

    private static bool ContainsAsPhrase(string normalizedLower, string termLower)
    {
        var parts = termLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return false;
        var pattern = "(?<![\\p{L}\\p{M}0-9])" +
                      string.Join(@"\s+", parts.Select(Regex.Escape)) +
                      @"(?![\\p{L}\\p{M}0-9])";
        return Regex.IsMatch(normalizedLower, pattern, RegexOptions.CultureInvariant);
    }

    private static int FirstMatchIndex(string normalizedLower, string termLower)
    {
        var parts = termLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return int.MaxValue;
        var pattern = "(?<![\\p{L}\\p{M}0-9])" +
                      string.Join(@"\s+", parts.Select(Regex.Escape)) +
                      @"(?![\\p{L}\\p{M}0-9])";
        var m = Regex.Match(normalizedLower, pattern, RegexOptions.CultureInvariant);
        return m.Success ? m.Index : int.MaxValue;
    }
}
