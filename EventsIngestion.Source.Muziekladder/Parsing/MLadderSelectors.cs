namespace EventsIngestion.Source.Muziekladder.Parsing;

internal static class MLadderSelectors
{
    public const string RootScope = "main";
    public const string GigRow = "div.row.gig-row.gig";
    public const string DetailContainer = ".detail_container";
    public const string GigAnchorHrefContains = "/gig/";
    public const string GigPageRoot = "main.event_full";
    public const string Headline = ".headline";
    public const string DateInfo = ".date-info";
    public const string LocationInfo = ".location-info";
    public const string LocationBlock = "div.location";
    public const string DescriptionWithItemProp = "p.description[itemprop='description']";
    public const string EventLink = "a.event_link";
    public const string CityName = ".city_name";
    public const string CountryName = ".country_name";
}
