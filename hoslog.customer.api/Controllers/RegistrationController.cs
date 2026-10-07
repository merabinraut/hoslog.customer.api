using Asp.Versioning;
using hoslog.customer.api.Filters;
using hoslog.customer.api.Models.UserRegistration;
using hoslog.customer.business.Conversion;
using hoslog.customer.business.UserRegistration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hoslog.customer.api.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1.0")]
    [AllowAnonymous]
    public class RegistrationController : BaseApiController
    {
        private readonly IConversionBusiness _conversion;
        public readonly IUserRegistrationBusiness _userRegistrationBusiness;
        public RegistrationController(IConversionBusiness conversion, IUserRegistrationBusiness userRegistrationBusiness)
        {
            _conversion = conversion;
            _userRegistrationBusiness = userRegistrationBusiness;
        }
        /// <summary>
        /// User registration to create new user
        /// </summary>
        /// <param name="mappedRequest"></param>
        /// <returns>Return success/fail status</returns>
        /// <remarks>
        /// **Sample request body:**
        ///
        ///     {
        ///             "actionPlatform": "web",
        ///             "nickName": "Nick name which one is required to save to the registration",
        ///             "mobileNumber": "Mobile number of 11 number",
        ///             "isCheckedTermsAndCondition": true/false which is bool parameter
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Success</response>
        /// <response code="401">Failed/Unauthorized</response>
        [LogRequest]
        [LogResponse]
        [HttpPost("register-user")]
        public async Task<IActionResult> UserRegistration(UserRegistrationModel mappedRequest)
        {
            var session = await _conversion.GetRequiredAsync(HttpContext);
            var (statusCode, response) = await _userRegistrationBusiness.UserRegistrationAsync(mappedRequest, session);
            return HandleResponseFromStatusCode(statusCode, response);
        }
        /// <summary>
        /// OTP verification after user registration complete
        /// </summary>
        /// <param name="mappedRequest"></param>
        /// <returns>Return success/fail status</returns>
        /// <remarks>
        /// **Sample request body:**
        ///
        ///     {
        ///           "actionPlatform": "web",
        ///           "otpCode": "Which is sent to the email",
        ///           "mobileNumber": "Mobile number of 11 number",
        ///           "holdCustomerId": "Encrypted hold customer id which is given on the response after user registration complete",
        ///           "processId": "Process Id which is given on user registration response"                                              
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Success</response>
        /// <response code="401">Failed/Unauthorized</response>
        [LogRequest]
        [LogResponse]
        [HttpPost("validate-user-registration-otp")]
        public async Task<IActionResult> UserRegistrationValidateOTP(UserRegistrationValidationModel mappedRequest)
        {
            var session = await _conversion.GetRequiredAsync(HttpContext);
            var (statusCode, response) = await _userRegistrationBusiness.UserRegistrationValidationAsync(mappedRequest, session);
            return HandleResponseFromStatusCode(statusCode, response);
        }
        /// <summary>
        /// Resend OTP if otp expire or on late entry. 
        /// </summary>
        /// <param name="mappedRequest"></param>
        /// <returns>Return success/fail status</returns>
        /// <remarks>
        /// **Sample request body:**
        ///
        ///     {
        ///           "actionPlatform": "web",
        ///            "holdCustomerId": "Encrypted hold customer id which is given during registration complete",
        ///            "mobileNumber": "11 digits mobile number"               
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Success</response>
        /// <response code="401">Failed/Unauthorized</response>
        [LogRequest]
        [LogResponse]
        [HttpPost("resend-user-registration-otp")]
        public async Task<IActionResult> UserResendOTP(ResendOTPRequestModel mappedRequest)
        {
            var (statusCode, response) = await _userRegistrationBusiness.UserResendOTPAsync(mappedRequest);
            return HandleResponseFromStatusCode(statusCode, response);
        }
        /// <summary>
        /// Create user password after otp verification success.
        /// </summary>
        /// <param name="mappedRequest"></param>
        /// <returns>Return success/fail status</returns>
        /// <remarks>
        /// **Sample request body:**
        ///
        ///     {
        ///             "customerId": "Encrypted customer ID given on the response of OTP verificaiton",
        ///             "mobileNumber": "11 digits of mobile number",
        ///             "processId": "Process ID whih is given on the response of the OTP verificaiton",
        ///             "password": "Password to set on the user credentials",
        ///             "confirmPassword": "Confirm password which is same as password",
        ///             "actionPlatform": "web"           
        ///            
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Success</response>
        /// <response code="401">Failed/Unauthorized</response>
        [LogRequest]
        [LogResponse]
        [HttpPost("create-user-password")]
        public async Task<IActionResult> CreateUserCredentials(CreatePasswordModel mappedRequest)
        {
            var (statusCode, response) = await _userRegistrationBusiness.CreateUserCredentialsAsync(mappedRequest);
            return HandleResponseFromStatusCode(statusCode, response);
        }
        /// <summary>
        /// Forgot password  if user forgot the password and reset as user need .
        /// </summary>
        /// <param name="mappedRequest"></param>
        /// <returns>Return success/fail status</returns>
        /// <remarks>
        /// **Sample request body:**
        ///
        ///     {
        ///              "actionPlatform": "web",
        ///              "mobileNumber": "11 digits mobile number which is registered on the user credentials."          
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Success</response>
        /// <response code="401">Failed/Unauthorized</response>
        [LogRequest]
        [LogResponse]
        [HttpPost("forgot-user-password")]
        public async Task<IActionResult> ForgotUserPassword(ForgotPasswordRequestModel mappedRequest)
        {
            var (statusCode, response) = await _userRegistrationBusiness.ForgotUserPasswordAsync(mappedRequest);
            return HandleResponseFromStatusCode(statusCode, response);
        }
        /// <summary>
        /// OTP verification which is sent to an email address after forgot password sent to .
        /// </summary>
        /// <param name="mappedRequest"></param>
        /// <returns>Return success/fail status</returns>
        /// <remarks>
        /// **Sample request body:**
        ///
        ///     {
        ///              "customerId": "Encrypted Customer ID,
        ///              "mobileNumber": "11 Digits mobile number",
        ///              "processId": "Unique process id provided by forgot password",
        ///              "actionPlatform": "web",
        ///              "otpCode": "OTP code which is sent to an email address"    
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Success</response>
        /// <response code="401">Failed/Unauthorized</response>
        [LogRequest]
        [LogResponse]
        [HttpPost("forgot-password-otp-verification")]
        public async Task<IActionResult> ForgotPasswordOTPVerification(ForgotPasswordOTPVerificatonRequestModel mappedRequest)
        {
            var (statusCode, response) = await _userRegistrationBusiness.ForgotPasswordOTPVerificationAsync(mappedRequest);
            return HandleResponseFromStatusCode(statusCode, response);
        }
        /// <summary>
        ///forgot password resend OTP request
        /// </summary>
        /// <param name="mappedRequest"></param>
        /// <returns>Return success/fail status</returns>
        /// <remarks>
        /// **Sample request body:**
        ///
        ///     {
        ///                "actionPlatform": "test",
        ///                "customerId": "Encrypted Customer ID which is given on the forgot password response",
        ///                "mobileNumber": "11 digits mobile number "
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Success</response>
        /// <response code="401">Failed/Unauthorized</response>
        [LogRequest]
        [LogResponse]
        [HttpPost("forgot-password-resend-otp-request")]
        public async Task<IActionResult> ForgotPasswordResendOTP(ForgotPasswordResendOTPModel mappedRequest)
        {
            var (statusCode, response) = await _userRegistrationBusiness.ForgotPasswordResendOTPAsync(mappedRequest);
            return HandleResponseFromStatusCode(statusCode, response);
        }
        /// <summary>
        ///Set password for forgot password
        /// </summary>
        /// <param name="mappedRequest"></param>
        /// <returns>Return success/fail status</returns>
        /// <remarks>
        /// **Sample request body:**
        ///
        ///     {
        ///                           "actionPlatform": "web",
        ///                           "customerId": "Encrypted Customer ID.
        ///                           "otpCode": "OTP code which is provided to the Email address.
        ///                           "mobileNumber": "11 digits mobile number.",
        ///                           "processId": "Unique process ID provided to the OTP verification"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">Success</response>
        /// <response code="401">Failed/Unauthorized</response>
        [LogRequest]
        [LogResponse]
        [HttpPost("set-forgot-password")]
        public async Task<IActionResult> ForgotPasswordSetOTP(ForgotSetPasswordModel mappedRequest)
        {
            var (statusCode, response) = await _userRegistrationBusiness.ForgotPasswordSetOTPAsync(mappedRequest);
            return HandleResponseFromStatusCode(statusCode, response);
        }
    }
}
