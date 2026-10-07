namespace hoslog.customer.common.Models.Middleware.AmazonCognitoModel.Auth
{
    public class SignOutModel
    {
        public class Request
        {
            public string AccessToken { get; set; } = null!;
        }
    }
}
