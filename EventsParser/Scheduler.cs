namespace EventsParser;

internal static class Scheduler
{
    public static async Task RunLoopAsync(
        bool runOnStart,
        Func<CancellationToken, Task> runOnce,
        CancellationToken cancellationToken)
    {
        var tz = GetNetherlandsTimeZone();
        Console.WriteLine($"Schedule mode: daily at 12:00 ({tz.Id}). Ctrl+C to stop.");

        if (runOnStart)
        {
            try
            {
                await runOnce(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }

            if (cancellationToken.IsCancellationRequested)
                return;
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            var nextUtc = GetNextNetherlandsNoonUtc(DateTime.UtcNow, tz);
            var wait = nextUtc - DateTime.UtcNow;
            if (wait < TimeSpan.Zero)
                wait = TimeSpan.Zero;
            Console.WriteLine($"Next run (Netherlands noon): {FormatNetherlandsLocal(nextUtc, tz)} (in {wait:hh\\:mm\\:ss})");
            try
            {
                await Task.Delay(wait, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                await runOnce(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
    }

    private static TimeZoneInfo GetNetherlandsTimeZone()
    {
        foreach (var id in new[] { "Europe/Amsterdam", "W. Europe Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        throw new InvalidOperationException("Could not resolve Netherlands time zone (Europe/Amsterdam).");
    }

    private static DateTime GetNextNetherlandsNoonUtc(DateTime utcNow, TimeZoneInfo nlTz)
    {
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, nlTz);
        var noonToday = new DateTime(localNow.Year, localNow.Month, localNow.Day, 12, 0, 0, DateTimeKind.Unspecified);
        var targetLocal = localNow < noonToday ? noonToday : noonToday.AddDays(1);
        return TimeZoneInfo.ConvertTimeToUtc(targetLocal, nlTz);
    }

    private static string FormatNetherlandsLocal(DateTime utcInstant, TimeZoneInfo nlTz)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(utcInstant, nlTz);
        return $"{local:yyyy-MM-dd HH:mm} ({nlTz.Id})";
    }
}
