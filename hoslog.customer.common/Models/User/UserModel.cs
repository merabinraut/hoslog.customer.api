namespace hoslog.customer.common.Models.User
{
    public class UserDetailModel
    {
        public string? AgentId { get; set; }
        public string? NickName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MobileNumber { get; set; }
        public string? DOB { get; set; }
        public string? EmailAddress { get; set; }
        public string? Gender { get; set; }
        public string? PreferredLocation { get; set; }
        public string? PostalCode { get; set; }
        public string? Prefecture { get; set; }
        public string? City { get; set; }
        public string? Street { get; set; }
        public string? ResidenceNumber { get; set; }
        public string? ProfileImage { get; set; }
        public string? PointsCategoryId { get; set; }
        public string? UserId { get; set; }
        public string? LoginId { get; set; }
        public byte[]? Password { get; set; }
        public string? FailedLoginAttempt { get; set; }
        public string? IsPasswordForceful { get; set; }
        public string? LastPasswordChangedDate { get; set; }
        public string? LastLoginDate { get; set; }
        public string? Session { get; set; }
        public string? RoleType { get; set; }
        public string? IsForcefulLogout { get; set; }
    }
}
