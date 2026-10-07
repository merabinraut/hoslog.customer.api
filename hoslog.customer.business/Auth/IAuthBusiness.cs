using hoslog.customer.common.Models.Auth;
using hoslog.customer.shared.Auth;
using System.Net;
using hoslog.customer.common.Models.Conversion;

namespace hoslog.customer.business.Auth
{
    public interface IAuthBusiness
    {
        Task<(HttpStatusCode, object)> Login(LoginRequestModel request,ConversionSessionContext session);
        Task<(HttpStatusCode, object)> Refresh(RefreshTokenRequestModel request);
        Task<bool> ValidateTokenAsync(string accessToken, string userId, string roleType);
        Task<(HttpStatusCode, object)> LogoutAsync();
        Task<AuthDbResponse> CreateOtpAsync(
            string username,
            string purpose,
            string otpHash,
            string otpCode,
            string traceId,
            string cognitoAuthSession
        );

        Task<AuthDbResponse> VerifyOtpWithOutTransactionAsync(
                    string username,
                    string purpose,
                    string otpCode,
                    string submittedOtpHash = ""
                );
    }
}