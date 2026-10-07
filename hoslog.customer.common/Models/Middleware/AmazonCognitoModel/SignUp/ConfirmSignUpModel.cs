namespace hoslog.customer.common.Models.Middleware.AmazonCognitoModel.SignUp
{
    public class ConfirmSignUpModel
    {
        public class Request
        {
            public string Username { get; set; } = null!;
            public string ConfirmationCode { get; set; } = null!;
        }
    }
}
