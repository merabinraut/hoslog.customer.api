namespace hoslog.customer.common.Models.Middleware.AmazonCognitoModel.SignUp
{
    public class ResendSignUpConfirmationCodeModel
    {
        public class Request
        {
            public string Username { get; set; } = null!;
        }
    }
}
