namespace hoslog.customer.common.Models.Middleware.AmazonCognitoModel.Auth
{
    public class RefreshModel
    {
        public class Request
        {
            public string Username { get; set; } = null!;
            public string RefreshToken { get; set; } = null!;
        }

        public class Response
        {
            public string? TokenType { get; set; }
            public string? IdToken { get; set; }
            public string? AccessToken { get; set; }
            public string? RefreshToken { get; set; }
            public int ExpiresIn { get; set; }
        }
    }
}
