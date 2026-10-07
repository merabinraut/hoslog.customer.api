using hoslog.customer.shared;
using hoslog.customer.shared.Auth;

namespace hoslog.customer.domain.Auth
{
    public interface IAuthRepository
    {
        Task<TokenDetailsCommon> GetTokenDetail(GetOrExpireTokenRequestCommon request);
        Task<CommonDBResponse> ManageToken(ManageTokenRequestCommon request);
        Task<CommonDBResponse> ExpireToken(GetOrExpireTokenRequestCommon request);
        Task<AuthDbResponse> CreateOtpAsync(
            string username,
            string purpose,
            string otpHash,
            string otpCode,
            string traceId,
            string cognitoAuthSession
        );

        Task<AuthDbResponse> VerifyOtpAsync(
            string username,
            string purpose,
            string otpCode,
            string submittedOtpHash = ""
        );

        Task<AuthDbResponse> VerifyOtpWithOutTransactionAsync(
            string username,
            string purpose,
            string otpCode,
            string submittedOtpHash = ""
        );
    }
}