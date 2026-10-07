namespace hoslog.customer.common.Models.IPService;

public sealed class CustomerIpLocation
{
    public string? ipAddress { get; set; }

    public string locationSource { get; set; } = "unknown";

    public bool isApproximate { get; set; } = true;

    public string? continentCode { get; set; }

    public string? countryCode { get; set; }

    public string? countryName { get; set; }

    public string? regionCode { get; set; }

    public string? regionName { get; set; }

    public string? cityName { get; set; }

    public string? postalCode { get; set; }

    public double? latitude { get; set; }

    public double? longitude { get; set; }

    public int? accuracyRadiusKilometers { get; set; }

    public string? timeZone { get; set; }
}

public sealed class DefaultCustomerLocationOptions
{
    public const string SectionName = "DefaultCustomerLocation";

    public string? countryCode { get; init; }

    public string? countryName { get; init; }

    public string? regionCode { get; init; }

    public string? regionName { get; init; }

    public string? cityName { get; init; }

    public string? postalCode { get; init; }

    public double? latitude { get; init; }

    public double? longitude { get; init; }

    public string? timeZone { get; init; }
}