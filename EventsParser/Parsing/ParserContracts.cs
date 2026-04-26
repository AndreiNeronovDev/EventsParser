namespace EventsParser.Parsing;

public readonly record struct AgendaListRow(string Url, string Title);

public interface IAgendaParser
{
    IReadOnlyList<AgendaListRow> ExtractRows(string html, Uri pageUri, out int detailContainers);
}

public interface IEventParser
{
    EventDto? Parse(string html, Uri eventPageUri);
}

public interface ISiteProfile
{
    string SiteKey { get; }
    string DefaultAgendaUrl { get; }
    IAgendaParser CreateAgendaParser();
    IEventParser CreateEventParser();
}
