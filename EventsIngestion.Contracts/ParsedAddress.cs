namespace EventsIngestion.Contracts;

/// <summary>
/// Normalized address and geographic position parsed from a source.
/// </summary>
public sealed record ParsedAddress
{
    /// <summary>
    /// Gets the country name.
    /// </summary>
    public string? Country { get; init; }

    /// <summary>
    /// Gets the ISO country code when the source provides it.
    /// </summary>
    public string? CountryCode { get; init; }

    /// <summary>
    /// Gets the region, province, or state.
    /// </summary>
    public string? Region { get; init; }

    /// <summary>
    /// Gets the city name.
    /// </summary>
    public string? City { get; init; }

    /// <summary>
    /// Gets the street address or free-form address line.
    /// </summary>
    public string? AddressLine { get; init; }

    /// <summary>
    /// Gets the postal code.
    /// </summary>
    public string? PostalCode { get; init; }

    /// <summary>
    /// Gets the latitude in decimal degrees.
    /// </summary>
    public decimal? Latitude { get; init; }

    /// <summary>
    /// Gets the longitude in decimal degrees.
    /// </summary>
    public decimal? Longitude { get; init; }
}
