namespace hoslog.customer.shared.Auth
{
    public class ManageRefreshTokenRequestCommon : CommonDBRequest
    {
        public string? traceId { get; set; }
        public string? refreshToken { get; set; }
        public DateTime refreshTokenExpiryTime { get; set; }
    }
}
