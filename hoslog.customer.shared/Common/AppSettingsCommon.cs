namespace hoslog.customer.shared.Common
{
    public class AppSettingsCommon
    {
        public JWTCommon JWT { get; set; }
        public SecurityKeyCommon SecurityKey { get; set; }
    }

    public class JWTCommon
    {
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int AccessTokenValidityInMinutes { get; set; }
        public int RefreshTokenValidityMinutes { get; set; }
    }

    public class SecurityKeyCommon
    {
        public string Key { get; set; }
        public string IV { get; set; }
        public string SecretKey { get; set; }
    }
}
