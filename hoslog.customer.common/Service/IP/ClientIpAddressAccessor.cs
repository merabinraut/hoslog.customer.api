using System.Net;
using Microsoft.AspNetCore.Http;

namespace hoslog.customer.common.Service.IP;

public interface IClientIpAddressAccessor
{
    IPAddress? GetClientIpAddress(HttpContext httpContext);
}

public sealed class ClientIpAddressAccessor : IClientIpAddressAccessor
{
    public IPAddress? GetClientIpAddress(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var forwardedFor =
            httpContext.Request.Headers["X-Forwarded-For"].ToString();

        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            var firstIpText = forwardedFor
                .Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries)
                .FirstOrDefault();

            if (IPAddress.TryParse(firstIpText, out var clientIp))
            {
                return NormalizeIpAddress(clientIp);
            }
        }

        var remoteIp = httpContext.Connection.RemoteIpAddress;

        return remoteIp is null
            ? null
            : NormalizeIpAddress(remoteIp);
    }

    private static IPAddress NormalizeIpAddress(IPAddress ipAddress)
    {
        return ipAddress.IsIPv4MappedToIPv6
            ? ipAddress.MapToIPv4()
            : ipAddress;
    }
}