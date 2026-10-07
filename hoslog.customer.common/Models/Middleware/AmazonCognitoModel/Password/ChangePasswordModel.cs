namespace hoslog.customer.common.Models.Middleware.AmazonCognitoModel.Password
{
    public class ChangePasswordModel
    {
        public class Request
        {
            public string AccessToken { get; set; } = null!;
            public string OldPassword { get; set; } = null!;
            public string NewPassword { get; set; } = null!;
        }
    }
}
