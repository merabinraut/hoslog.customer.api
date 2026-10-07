namespace hoslog.customer.common.Models.Middleware.AmazonCognitoModel.Password
{
    public class ForgotPasswordModel
    {
        public class Request
        {
            public string Username { get; set; } = null!;
        }
    }

    public class ResendForgotPasswordConfirmationCodeModel
    {
        public class Request
        {
            public string Username { get; set; } = null!;
        }
    }

    public class ConfirmForgotPasswordModel
    {
        public class Request
        {
            public string Username { get; set; } = null!;
            public string ConfirmationCode { get; set; } = null!;
            public string NewPassword { get; set; } = null!;
        }
    }
}
