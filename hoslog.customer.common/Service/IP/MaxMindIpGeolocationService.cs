using System.Net;
using hoslog.customer.common.Models.IPService;
using hoslog.customer.common.Utilities;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace hoslog.customer.common.Service.IP;

public interface IIpGeolocationService
{
    CustomerIpLocation? FindLocation(IPAddress ipAddress);
}

public sealed class MaxMindIpGeolocationService :
    IIpGeolocationService,
    IDisposable
{
    private readonly DatabaseReader _databaseReader;
    private readonly ILogger<MaxMindIpGeolocationService> _logger;

    public MaxMindIpGeolocationService(
        IOptions<MaxMindOptions> options,
        IWebHostEnvironment environment,
        ILogger<MaxMindIpGeolocationService> logger)
    {
        _logger = logger;

        var configuredPath = options.Value.databasePath;

        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            throw new InvalidOperationException(
                "MaxMind database path is not configured.");
        }

        var databasePath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(
                environment.ContentRootPath,
                configuredPath);

        if (!File.Exists(databasePath))
        {
            throw new FileNotFoundException(
                $"MaxMind database was not found: {databasePath}",
                databasePath);
        }

        _databaseReader = new DatabaseReader(databasePath);
    }

    public CustomerIpLocation? FindLocation(IPAddress ipAddress)
    {
        ipAddress = IpAddressUtility.Normalize(ipAddress);

        if (IpAddressUtility.IsPrivateOrLocal(ipAddress))
        {
            return null;
        }

        try
        {
            var result = _databaseReader.City(
                ipAddress.ToString());

            return new CustomerIpLocation
            {
                ipAddress = ipAddress.ToString(),
                locationSource = "maxmind_ip",
                isApproximate = true,

                continentCode =
                    result.Continent?.Code,

                countryCode =
                    result.Country?.IsoCode,

                countryName =
                    result.Country?.Name,

                regionCode =
                    result.MostSpecificSubdivision?.IsoCode,

                regionName =
                    result.MostSpecificSubdivision?.Name,

                cityName =
                    result.City?.Name,

                postalCode =
                    result.Postal?.Code,

                latitude =
                    result.Location?.Latitude,

                longitude =
                    result.Location?.Longitude,

                accuracyRadiusKilometers =
                    result.Location?.AccuracyRadius,

                timeZone =
                    result.Location?.TimeZone
            };
        }
        catch (AddressNotFoundException)
        {
            _logger.LogInformation(
                "MaxMind location was not found for IP address {IpAddress}.",
                ipAddress);

            return null;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "MaxMind lookup failed for IP address {IpAddress}.",
                ipAddress);

            throw;
        }
    }

    public void Dispose()
    {
        _databaseReader.Dispose();
    }
}