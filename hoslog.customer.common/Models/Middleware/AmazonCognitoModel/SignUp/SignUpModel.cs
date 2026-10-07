namespace hoslog.customer.common.Models.Middleware.AmazonCognitoModel.SignUp
{
    public class SignUpModel
    {
        public class Request
        {
            public string Username { get; set; } = null!;
            public string Password { get; set; } = null!;
            public List<AttributeType> AttributeType { get; set; } = new List<AttributeType>();
        }

        public class AttributeType
        {
            public string? Name { get; set; }
            public string? Value { get; set; }
        }

        public class Response
        {
            public string? UserSub { get; set; }
        }

        public class AdminCreateUserResponse
        {
            public string? Name { get; set; }
            public string? Value { get; set; }
        }
    }
}