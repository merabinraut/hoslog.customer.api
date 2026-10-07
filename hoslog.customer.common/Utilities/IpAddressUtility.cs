using System.Net;
using System.Net.Sockets;

namespace hoslog.customer.common.Utilities;

public static class IpAddressUtility
{
    public static IPAddress Normalize(IPAddress ipAddress)
    {
        return ipAddress.IsIPv4MappedToIPv6
            ? ipAddress.MapToIPv4()
            : ipAddress;
    }

    public static bool IsPrivateOrLocal(IPAddress ipAddress)
    {
        ipAddress = Normalize(ipAddress);

        if (IPAddress.IsLoopback(ipAddress))
        {
            return true;
        }

        if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (ipAddress.Equals(IPAddress.IPv6Any) ||
                ipAddress.Equals(IPAddress.IPv6None) ||
                ipAddress.IsIPv6LinkLocal ||
                ipAddress.IsIPv6Multicast ||
                ipAddress.IsIPv6SiteLocal)
            {
                return true;
            }

            var bytes = ipAddress.GetAddressBytes();

            // Unique local IPv6 address: fc00::/7
            return (bytes[0] & 0xFE) == 0xFC;
        }

        if (ipAddress.AddressFamily != AddressFamily.InterNetwork)
        {
            return true;
        }

        var addressBytes = ipAddress.GetAddressBytes();

        var first = addressBytes[0];
        var second = addressBytes[1];

        return
            // 0.0.0.0/8
            first == 0 ||

            // 10.0.0.0/8
            first == 10 ||

            // 100.64.0.0/10 - carrier-grade NAT
            first == 100 && second is >= 64 and <= 127 ||

            // 127.0.0.0/8 - loopback
            first == 127 ||

            // 169.254.0.0/16 - link local
            first == 169 && second == 254 ||

            // 172.16.0.0/12
            first == 172 && second is >= 16 and <= 31 ||

            // 192.168.0.0/16
            first == 192 && second == 168 ||

            // Multicast and reserved
            first >= 224;
    }
}