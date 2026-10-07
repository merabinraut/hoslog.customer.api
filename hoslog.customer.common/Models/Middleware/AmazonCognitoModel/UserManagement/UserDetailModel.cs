namespace hoslog.customer.common.Models.Middleware.AmazonCognitoModel.UserManagement
{
    public class UserDetailModel
    {
        public class Request
        {
            public string Username { get; set; } = null!;
        }

        public class Response
        {
            public string? Sub { get; set; }
            public string? Email { get; set; }
            public string? PhoneNumber { get; set; }
            public string? UserStatus { get; set; }
            public string? Username { get; set; }
        }
    }
}