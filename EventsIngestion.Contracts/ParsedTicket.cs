namespace EventsIngestion.Contracts;

/// <summary>
/// Normalized ticket offer parsed from a source.
/// </summary>
public sealed record ParsedTicket
{
    /// <summary>
    /// Gets the ticket name, tier, or label.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets the parsed numeric price when available.
    /// </summary>
    public decimal? Price { get; init; }

    /// <summary>
    /// Gets the currency code or symbol when available.
    /// </summary>
    public string? Currency { get; init; }

    /// <summary>
    /// Gets the original price text from the source when numeric parsing is not reliable.
    /// </summary>
    public string? PriceText { get; init; }

    /// <summary>
    /// Gets the ticket URL.
    /// </summary>
    public string? Url { get; init; }

    /// <summary>
    /// Gets the ticket status reported by the source.
    /// </summary>
    public string? Status { get; init; }

    /// <summary>
    /// Gets whether the source marks this ticket as free.
    /// </summary>
    public bool? IsFree { get; init; }
}
