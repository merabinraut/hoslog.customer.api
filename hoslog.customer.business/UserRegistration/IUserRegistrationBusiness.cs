using hoslog.customer.api.Models.UserRegistration;
using hoslog.customer.shared.UserRegistration;
using System.Net;
using hoslog.customer.common.Models.Conversion;

namespace hoslog.customer.business.UserRegistration
{
    public interface IUserRegistrationBusiness
    {
        Task<(HttpStatusCode, object)> ChangeCustomerPasswordAsync(ChangeCustomerPasswordRequestModel mappedRequest);
        Task<(HttpStatusCode, object)> CreateUserCredentialsAsync(CreatePasswordModel mappedRequest);
        Task<(HttpStatusCode, object)> ForgotPasswordOTPVerificationAsync(ForgotPasswordOTPVerificatonRequestModel mappedRequest);
        Task<(HttpStatusCode, object)> ForgotPasswordResendOTPAsync(ForgotPasswordResendOTPModel mappedRequest);
        Task<(HttpStatusCode, object)> ForgotPasswordSetOTPAsync(ForgotSetPasswordModel mappedRequest);
        Task<(HttpStatusCode, object)> ForgotUserPasswordAsync(ForgotPasswordRequestModel mappedRequest);
        Task<(HttpStatusCode, object)> UserRegistrationAsync(UserRegistrationModel mappedRequest,ConversionSessionContext session);
        Task<(HttpStatusCode, object)> UserRegistrationValidationAsync(UserRegistrationValidationModel mappedRequest,ConversionSessionContext session);
        Task<(HttpStatusCode, object)> UserResendOTPAsync(ResendOTPRequestModel mappedRequest);
        Task<HoldCustomerDetailCommon> HoldCustomerDetail(string mobileNumber, string holdCustomerId, string processId = "", string processType = "",string expiryTime="");
    }
}
