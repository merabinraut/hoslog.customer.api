namespace hoslog.customer.shared.Auth
{
    public class ManageAccessTokenRequestCommon : CommonDBRequest
    {
        public string? userId { get; set; }
        public string? loginId { get; set; }
        public string? accessToken { get; set; }
        public DateTime accessTokenExpiryTime { get; set; }
        public string? traceId { get; set; }
    }
}
