namespace hoslog.customer.api.Models
{
    public class AppSettingsModel
    {
        public JWT JWT { get; set; }
        public SecurityKey SecurityKey { get; set; }
    }

    public class JWT
    {
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int AccessTokenValidityInMinutes { get; set; }
        public int RefreshTokenValidityMinutes { get; set; }
    }

    public class SecurityKey
    {
        public string Key { get; set; }
        public string IV { get; set; }
        public string SecretKey { get; set; }
    }
}
