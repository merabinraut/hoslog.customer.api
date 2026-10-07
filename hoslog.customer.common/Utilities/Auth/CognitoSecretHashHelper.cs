using System.Security.Cryptography;
using System.Text;

namespace hoslog.customer.common.Utilities;

public static class CognitoSecretHashHelper
{
    public static string CalculateSecretHash(
        string username,
        string clientId,
        string clientSecret
    )
    {
        var message = username + clientId;

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(clientSecret));

        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));

        return Convert.ToBase64String(hashBytes);
    }
}