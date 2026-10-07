namespace hoslog.customer.shared.Auth
{
    public class TokenDetailCommon
    {
        public string? sno { get; set; }
        public string? traceId { get; set; }
        public string? userId { get; set; }
        public string? loginId { get; set; }
        public string? accessToken { get; set; }
        public DateTime accessTokenExpiryTime { get; set; }
        public string? refreshToken { get; set; }
        public DateTime refreshTokenExpiryTime { get; set; }
        public string? createdBy { get; set; }
        public string? createdDate { get; set; }
        public string? createdUTCDate { get; set; }
        public string? createdPlatform { get; set; }
        public string? createdIP { get; set; }
        public string? updatedBy { get; set; }
        public string? updatedDate { get; set; }
        public string? updatedUTCDate { get; set; }
        public string? updatedPlatform { get; set; }
        public string? updatedIP { get; set; }
    }
}
