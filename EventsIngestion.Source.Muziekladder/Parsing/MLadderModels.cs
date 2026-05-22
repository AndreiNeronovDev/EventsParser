namespace EventsIngestion.Source.Muziekladder.Parsing;

internal sealed class MLadderEvent
{
    public string Title { get; set; } = "";
    public string Date { get; set; } = "";
    public string? StartTime { get; set; }
    public string? DoorsTime { get; set; }
    public string Description { get; set; } = "";
    public MLadderLocation Location { get; set; } = new();
    public List<string> Genres { get; set; } = [];
    public List<string> ImageUrls { get; set; } = [];
    public List<MLadderTicket> Tickets { get; set; } = [];
    public string? OriginalEventLink { get; set; }
    public List<string> Lineup { get; set; } = [];
}

internal sealed class MLadderLocation
{
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string City { get; set; } = "";
    public string Country { get; set; } = "";
    public string Address { get; set; } = "";
    public string VenueName { get; set; } = "";
}

internal sealed class MLadderTicket
{
    public string Url { get; set; } = "";
    public string? Price { get; set; }
}

internal readonly record struct AgendaListRow(string Url, string Title);
