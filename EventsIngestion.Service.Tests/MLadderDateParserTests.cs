using EventsIngestion.Source.Muziekladder.Parsing;

namespace EventsIngestion.Service.Tests;

public sealed class MLadderDateParserTests
{
    [Test]
    public void ParseStartDate_ParsesDutchWeekdayDate()
    {
        var result = MLadderDateParser.ParseStartDate("vrijdag 22 mei 2026");

        Assert.That(result, Is.EqualTo(new DateTime(2026, 5, 22)));
    }

    [Test]
    public void ParseStartDate_ParsesDutchDateWithoutWeekday()
    {
        var result = MLadderDateParser.ParseStartDate("22 mei 2026");

        Assert.That(result, Is.EqualTo(new DateTime(2026, 5, 22)));
    }

    [Test]
    public void MapStartsAt_CombinesParsedDateAndTime()
    {
        var startDate = MLadderDateParser.ParseStartDate("vrijdag 22 mei 2026");

        var result = MLadderDateParser.MapStartsAt(startDate, "20:30");

        Assert.Multiple(() =>
        {
            Assert.That(result?.Year, Is.EqualTo(2026));
            Assert.That(result?.Month, Is.EqualTo(5));
            Assert.That(result?.Day, Is.EqualTo(22));
            Assert.That(result?.Hour, Is.EqualTo(20));
            Assert.That(result?.Minute, Is.EqualTo(30));
        });
    }

    [Test]
    public void MapDateTime_CombinesParsedDateAndDoorsTime()
    {
        var startDate = MLadderDateParser.ParseStartDate("vrijdag 22 mei 2026");

        var result = MLadderDateParser.MapDateTime(startDate, "19:30");

        Assert.Multiple(() =>
        {
            Assert.That(result?.Year, Is.EqualTo(2026));
            Assert.That(result?.Month, Is.EqualTo(5));
            Assert.That(result?.Day, Is.EqualTo(22));
            Assert.That(result?.Hour, Is.EqualTo(19));
            Assert.That(result?.Minute, Is.EqualTo(30));
        });
    }
}
