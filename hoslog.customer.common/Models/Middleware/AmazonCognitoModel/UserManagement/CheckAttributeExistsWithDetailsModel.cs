namespace hoslog.customer.common.Models.Middleware.AmazonCognitoModel.UserManagement
{
    public class CheckAttributeExistsWithDetailsModel
    {
        public class Request
        {
            public string attributeName { get; set; } = null!;
            public string attributeValue { get; set; } = null!;
        }

        public class Response
        {
            public List<User> Users { get; set; } = new List<User>();
        }

        public class User
        {
            public string? Username { get; set; }
            public List<Attribute> Attributes { get; set; } = new List<Attribute>();
            public string? UserStatus { get; set; }
        }
        public class Attribute
        {
            public string? Name { get; set; }
            public string? Value { get; set; }
        }
    }
}