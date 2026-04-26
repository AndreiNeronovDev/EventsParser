namespace EventsParser.Parsing;

public sealed class MuziekladderSiteProfile(MuziekladderSelectors? selectors = null) : ISiteProfile
{
    private readonly MuziekladderSelectors _selectors = selectors ?? MuziekladderSelectors.Default;

    public string SiteKey => "muziekladder";
    public string DefaultAgendaUrl => "https://muziekladder.nl/nl/muziek/";

    public IAgendaParser CreateAgendaParser() => new AgendaIndexParser(_selectors);
    public IEventParser CreateEventParser() => new GigDetailParser(_selectors);
}
