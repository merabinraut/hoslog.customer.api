namespace hoslog.customer.common.Models.Auth
{
    public class TokenPairModel
    {
        public string? scheme { get; set; }
        public string? accessToken { get; set; }
        public string? refreshToken { get; set; }
        public long? accessTokenExpiry { get; set; }
        public long? refreshTokenExpiry { get; set; }
        public bool? isPasswordForceful { get; set; }
    }
}
