namespace hoslog.customer.shared.UserRegistration
{
    public class UserRegistrationCommon : CommonDBRequest
    {
        public string? nickName { get; set; }
        public string? mobileNumber { get; set; }
        public string? dob { get; set; }
        public string? email { get; set; }
        public string? referCode { get; set; }
        public string? referType { get; set; }
    }

    public class UserRegistrationResponseCommon : CommonApiResponse
    {
        public string? holdCustomerId { get; set; }
        public string? mobileNumber { get; set; }
        public string? email { get; set; }
        public long expiryTime { get; set; }
        public string? processId { get; set; }
        public string? cognitoUserName { get; set; }
    }

    public class UserRegistrationVerificationRequestCommon : CommonDBRequest
    {
        public string? otpCode { get; set; }
        public string? mobileNumber { get; set; }
        public string? holdCustomerId { get; set; }
        public string? processId { get; set; }
        public string? cognitoUserId { get; set; }
    }

    public class UserRegistrationVerificationResponse : CommonApiResponse
    {
        public string? customerId { get; set; }
        public string? mobileNumber { get; set; }
        public string? email { get; set; }
        public string? processId { get; set; }
        public string? nickName { get; set; }
        public string? password { get; set; }
    }

    public class CreatePasswordRequestCommon : CommonDBRequest
    {
        public string? oldPassword { get; set; }
        public string? newPassword { get; set; }
        public string? confirmPassword { get; set; }
    }
    public class CreatePasswordResponseCommon : CommonApiResponse
    {
        public string? customerId { get; set; }
        public string? mobileNumber { get; set; }
    }

    #region Resend OTP
    public class ResendOTPRequestCommon : CommonDBRequest
    {
        public string? holdCustomerId { get; set; }
        public string? mobileNumber { get; set; }
    }

    public class ResendOTPResponsecommon : CommonApiResponse
    {
        public string? holdCustomerId { get; set; }
        public string? mobileNumber { get; set; }
        public string? processId { get; set; }
        public string? nickName { get; set; }
        public string? cognitoUsername { get; set; }
        public long expiryTime { get; set; }
    }
    #endregion
    #region Forgot Password
    public class ForgotPasswordResponseCommon : CommonApiResponse
    {
        public string? customerId { get; set; }
        public string? MobileNumber { get; set; }
        public long expiryTime { get; set; }
        public string? processId { get; set; }
        public string? nickName { get; set; }
        public string? cognitoUsername { get; set; }
    }
    public class ForgotPasswordVerificationResponseCommon : CommonApiResponse
    {
        public string? userId { get; set; }
        public string? customerId { get; set; }
        public string? mobileNumber { get; set; }
        public string? processId { get; set; }
        public string? nickName { get; set; }
        public string? cognitoUsername { get; set; }
    }
    public class ForgotPasswordVerificationResponse
    {
        public string? customerId { get; set; }
        public string? mobileNumber { get; set; }
        public string? processId { get; set; }
    }
    public class ForgotPasswordResendResponseCommon : CommonApiResponse
    {
        public string? mobileNumber { get; set; }
        public string? customerId { get; set; }
        public string? processId { get; set; }
        public string? nickName { get; set; }
        public string? cognitoUsername { get; set; }
    }
    public class ForgotSetPasswordCommon : CommonDBRequest
    {
        public string? customerId { get; set; }
        public string? password { get; set; }
        public string? confirmPassword { get; set; }
        public string? processId { get; set; }
    }
    public class ForgotSetPasswordResponseCommon : CommonApiResponse
    {
        public string? customerId { get; set; }
        public string? cognitoUsername { get; set; }
    }
    public class ForgotSetPasswordResponse
    {
        public string? customerId { get; set; }
    }
    #endregion
    #region ChangePassword
    public class ChangeCustomerPasswordResponseCommon : CommonApiResponse
    {
        public string? customerId { get; set; }
        public string? userId { get; set; }

    }
    #endregion

    public class HoldCustomerDetailCommon : CommonDBResponse
    {
        public string? holdCustomerId { get; set; }
        public string? nickName { get; set; }
        public string? cognitoUsername { get; set; }
        public string? mobileNumber { get; set; }
    }

    #region Remove Unconfirmed Customer Information
    public class RemoveUnconfirmedCustomerInformationCommon : CommonDBRequest
    {
        public string? nickName { get; set; }
        public string? exceptUserSno { get; set; }
    }
    #endregion
}
