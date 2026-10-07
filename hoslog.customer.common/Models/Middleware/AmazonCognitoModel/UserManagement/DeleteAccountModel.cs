namespace hoslog.customer.common.Models.Middleware.AmazonCognitoModel.UserManagement
{
    public class DeleteAccountModel
    {
        public class Request
        {
            public string AccessToken { get; set; } = null!;
        }
    }

    public class AdminDeleteAccountModel
    {
        public class Request
        {
            public string userName { get; set; } = null!;
        }
    }
}
