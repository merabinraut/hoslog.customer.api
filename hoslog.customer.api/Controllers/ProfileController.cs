using Asp.Versioning;
using hoslog.customer.api.Filters;
using hoslog.customer.api.Library;
using hoslog.customer.api.Models.UserRegistration;
using hoslog.customer.business.Profile;
using hoslog.customer.business.UserRegistration;
using hoslog.customer.common.Models.Auth;
using hoslog.customer.common.Models.User;
using hoslog.customer.shared;
using hoslog.customer.shared.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hoslog.customer.api.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1.0")]
    public class ProfileController : BaseApiController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProfileBusiness _profileBusiness;
        public readonly IUserRegistrationBusiness _userRegistrationBusiness;
        private readonly ImageHelper _imageHelper;
        public ProfileController(IProfileBusiness profileBusiness, IHttpContextAccessor httpContextAccessor, IUserRegistrationBusiness userRegistrationBusiness, ImageHelper imageHelper)
        {
            _profileBusiness = profileBusiness;
            _httpContextAccessor = httpContextAccessor;
            _userRegistrationBusiness = userRegistrationBusiness;
            _imageHelper = imageHelper;
        }
        /// <summary>
        /// This method is used to get user details.
        /// </summary>
        /// <returns></returns>
        //[LogRequest]
        //[LogResponse]
        [HttpGet("get-user-detail")]
        [Authorize]
        public async Task<IActionResult> GetUserDetails()
        {
            AppSettingData appSettingData = new AppSettingData();
            var userId = _httpContextAccessor.HttpContext?.User?.Claims.First(x => x.Type == UserClaimsName.UserId).Value;
            var awsSettingModel = appSettingData.GetAmazonSetting;
            var mappedAwsSetting = awsSettingModel.MapObject<AmazonSettingsCommon>();
            var (statusCode, response) = await _profileBusiness.GetUserDetails(userId, mappedAwsSetting);
            return HandleResponseFromStatusCode(statusCode, response);
        }

        [LogRequest]
        [LogResponse]
        [HttpPost("update-user-detail")]
        [Authorize]
        public async Task<IActionResult> UpdateUserDetails(UserRequestCommon objUserRequestCommon)
        {
            AppSettingData appSettingData = new AppSettingData();
            var awsSettingModel = appSettingData.GetAmazonSetting;
            var mappedAwsSetting = awsSettingModel.MapObject<AmazonSettingsCommon>();
            var (statusCode, response) = await _profileBusiness.UpdateUserDetails(objUserRequestCommon, mappedAwsSetting);
            return HandleResponseFromStatusCode(statusCode, response);
        }
        [LogRequest]
        [LogResponse]
        [HttpPost("upload-user-image")]
        [Authorize]
        public async Task<IActionResult> UploadImage(ProfileModel uploadImageObj)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.Claims.First(x => x.Type == UserClaimsName.UserId).Value;
            uploadImageObj.actionPlatform = _httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("ActionPlatform", out var headerValue) == true ? headerValue.FirstOrDefault() : null;

            string profileImage = uploadImageObj.profileImage;
            var (statusCode, response) = await _profileBusiness.SaveImage(profileImage, userId);
            return HandleResponseFromStatusCode(statusCode, response);
        }
        [LogRequest]
        [LogResponse]
        [HttpPost("get-presigned-url")]
        [Authorize]
        public async Task<IActionResult> GetPresignedUrl(string KeyName)
        {
            var (statusCode, response) = await _imageHelper.GetPresignedUrlAsync(KeyName);
            return HandleResponseFromStatusCode(statusCode, response);
        }

        [LogRequest]
        [LogResponse]
        [HttpPost("delete-account")]
        [Authorize]
        public async Task<IActionResult> DeleteUser()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.Claims.First(x => x.Type == UserClaimsName.UserId).Value;
            var (statusCode, response) = await _profileBusiness.DeleteUserAsync(userId);
            return HandleResponseFromStatusCode(statusCode, response);
        }

        /// <summary>
        ///Change password
        /// </summary>
        /// <param name="mappedRequest"></param>
        /// <returns>Return success/fail status</returns>
        /// <remarks>
        /// **Sample request body:**
        ///
        ///     {
        ///                    "actionPlatform": "web",
        ///                    "customerId": "Encrypted customer Id",
        ///                    "oldPassword": "Old password of customer which one is use to login.",
        ///                    "newPassword": "New password that want to change.",
        ///                    "confirmPassword": "Confirm password which is same as New password",
        ///                    "processId": "Unique process ID provided to the OTP verification"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Success</response>
        /// <response code="401">Failed/Unauthorized</response>
        [LogRequest]
        [LogResponse]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangeCustomerPassword(ChangeCustomerPasswordRequestModel mappedRequest)
        {
            var (statusCode, response) = await _userRegistrationBusiness.ChangeCustomerPasswordAsync(mappedRequest);
            return HandleResponseFromStatusCode(statusCode, response);
        }
    }
}
