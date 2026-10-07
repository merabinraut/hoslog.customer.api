namespace hoslog.customer.common.Models.Middleware.AmazonCognitoModel.Password
{
    public class SetPasswordModel
    {
        public class Request
        {
            public string Username { get; set; } = null!;
            public string Password { get; set; } = null!;
            public bool IsPermanent { get; set; } = false;
        }
    }
}
