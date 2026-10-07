namespace hoslog.customer.shared.Auth
{
    public class TokenDetailsCommon
    {
        public string? sno { get; set; }
        public string? userId { get; set; }
        public string? roleType { get; set; }
        public string? accessToken { get; set; }
        public long? accessTokenExpiryTime { get; set; }
        public long? accessTokenExpiryTimeUTC { get; set; }
        public string? refreshToken { get; set; }
        public long? refreshTokenExpiryTime { get; set; }
        public long? refreshTokenExpiryTimeUTC { get; set; }
        public string? createdBy { get; set; }
        public long? createdDate { get; set; }
        public long? createdUTCDate { get; set; }
        public string? createdPlatform { get; set; }
        public string? createdIP { get; set; }
        public string? updatedBy { get; set; }
        public long? updatedDate { get; set; }
        public long? updatedUTCDate { get; set; }
        public string? updatedPlatform { get; set; }
        public string? updatedIP { get; set; }
    }

    public class GetOrExpireTokenRequestCommon : CommonDBRequest
    {
        public string? roleType { get; set; }
    }

    public class ManageTokenRequestCommon : CommonDBRequest
    {
        public string? roleType { get; set; }
        public string? accessToken { get; set; }
        public long? accessTokenExpiryTimeMS { get; set; }
        public string? refreshToken { get; set; }
        public long? refreshTokenExpiryTimeMS { get; set; }
    }
}
