using hoslog.customer.api.Models.UserRegistration;
using hoslog.customer.common.Models.Middleware.AmazonCognito;
using hoslog.customer.shared.UserRegistration;

namespace hoslog.customer.domain.UserRegistration
{
    public interface IUserRegistrationRepository
    {
        Task<ChangeCustomerPasswordResponseCommon> ChangeCustomerPasswordAsync(CreatePasswordRequestCommon mappedRequest);
        Task<ForgotPasswordVerificationResponseCommon> ForgotPasswordOTPVerificationAsync(ForgotPasswordOTPVerificatonRequestModel mappedRequest);
        Task<ForgotPasswordResendResponseCommon> ForgotPasswordResendOTPAsync(ForgotPasswordResendOTPModel mappedRequest);
        Task<ForgotSetPasswordResponseCommon> ForgotPasswordSetOTPAsync(ForgotSetPasswordCommon request);
        Task<ForgotPasswordResponseCommon> ForgotUserPasswordAsync(ForgotPasswordRequestModel mappedRequest);
        Task<CreatePasswordResponseCommon> UserCreatePasswordAsync(CreatePasswordModel mappedRequest);
        Task<UserRegistrationResponseCommon> UserRegistrationAsync(UserRegistrationCommon mappedRequest);
        Task<UserRegistrationVerificationResponse> UserRegistrationVerificationOTP(UserRegistrationVerificationRequestCommon mappedRequest);
        Task<ResendOTPResponsecommon> UserResendOTPAsync(ResendOTPRequestCommon mappedRequest);
        Task<HoldCustomerDetailCommon> HoldCustomerDetail(string mobileNumber, string holdCustomerId, string processId = "", string processType = "",string expiryTime="");
        #region Remove Unconfirmed Customer Information
        Task<CommonResponse> RemoveUnconfirmedCustomerInformation(RemoveUnconfirmedCustomerInformationCommon request);
        #endregion
    }
}
