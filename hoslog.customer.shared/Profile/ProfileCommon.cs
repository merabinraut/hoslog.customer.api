using Microsoft.AspNetCore.Http;

namespace hoslog.customer.shared.Profile
{
    public class ProfileCommon
    {
        public string? nickName { get; set; }
        public string? firstName { get; set; }
        public string? lastName { get; set; }
        public string? mobileNumber { get; set; }
        public string? dob { get; set; }
        public string? emailAddress { get; set; }
        public string? gender { get; set; }
        public string? preferredLocation { get; set; }
        public string? postalCode { get; set; }
        public string? prefecture { get; set; }
        public string? city { get; set; }
        public string? street { get; set; }
        public string? residenceNumber { get; set; }
        public string? profileImage { get; set; }
        public int balance { get; set; }
        public string? referCode { get; set; }
    }
    public class ProfileCommonResponse
    {
        public string? nickName { get; set; }
        public string? firstName { get; set; }
        public string? lastName { get; set; }
        public string? mobileNumber { get; set; }
        public string? dob { get; set; }
        public string? emailAddress { get; set; }
        public string? referCode { get; set; }
        public string? gender { get; set; }
        public string? preferredLocation { get; set; }
        public string? postalCode { get; set; }
        public string? prefecture { get; set; }
        public string? city { get; set; }
        public string? street { get; set; }
        public string? residenceNumber { get; set; }
        public int balance { get; set; }
        public ImageResponse? image { get; set; }
    }

    #region update customer details
    public class UserRequestCommon
    {
        public string? firstName { get; set; }
        public string? lastName { get; set; }
        public string? dob { get; set; }
        public string? gender { get; set; }
        public string? postalCode { get; set; }
        public string? prefecture { get; set; }
        public string? city { get; set; }
        public string? street { get; set; }
        public string? residenceNumber { get; set; }
        public string? email { get; set; }
    }
    public class UseruploadImage
    {
        public IFormFile? profileImage { get; set; }
        //public string profileImagePath { get; set; }

    }

    #endregion
}
