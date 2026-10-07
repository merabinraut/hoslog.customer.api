using System.Security.Cryptography;
using System.Text;

namespace hoslog.customer.common.Utilities;

public static class OtpHashHelper
{
    public static string HashOtp(
        string username,
        string purpose,
        string otpCode,
        string secret
    )
    {
        if (string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException("OTP hash secret is missing.");

        username = NormalizeUsername(username);
        purpose = purpose.Trim().ToUpperInvariant();
        otpCode = otpCode.Trim();

        var payload = $"{username}|{purpose}|{otpCode}";
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(payloadBytes);

        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public static string NormalizeUsername(string username) => username.Trim().ToLower();
}