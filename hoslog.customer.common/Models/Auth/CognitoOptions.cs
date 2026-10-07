namespace hoslog.customer.common.Models.Auth;

public sealed class CognitoOptions
{
    public string Region { get; set; } = string.Empty;
    public string UserPoolId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string InternalOtpSecret { get; set; } = string.Empty;
    public string OtpHashSecret { get; set; } = string.Empty;
    public int OtpExpireMinutes { get; set; } = 10;
}