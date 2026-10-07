namespace hoslog.customer.common.Models.Middleware.AmazonCognitoModel.UserManagement
{
    public class UpdateUserAttributesModel
    {
        public class Request
        {
            public string AccessToken { get; set; } = null!;
            public List<AttributeType> AttributeType { get; set; } = new List<AttributeType>();
        }
        public class AttributeType
        {
            public string? Name { get; set; }
            public string? Value { get; set; }
        }
    }

    public class MarkEmailAsVerifiedModel
    {
        public class Request
        {
            public string Username { get; set; } = null!;
            public string Email { get; set; } = null!;
        }
    }

    public class MarkMobileNumberAsVerifiedModel
    {
        public class Request
        {
            public string Username { get; set; } = null!;
            public string MobileNumber { get; set; } = null!;
        }
    }
}
