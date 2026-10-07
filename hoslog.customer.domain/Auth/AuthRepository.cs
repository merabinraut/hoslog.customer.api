using Dapper;
using hoslog.customer.common.Utilities;
using hoslog.customer.shared;
using hoslog.customer.shared.Auth;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.domain.Auth
{
    public class AuthRepository : IAuthRepository
    {
        private readonly RepositoryDao _dao;
        private readonly SqlTransactionHandler _sqlTransactionHandler;
        public AuthRepository
        (
            IConfiguration configuration,
            SqlTransactionHandler sqlTransactionHandler
        )
        {
            _dao = new RepositoryDao(configuration);
            _sqlTransactionHandler = sqlTransactionHandler;
        }

        public async Task<TokenDetailsCommon> GetTokenDetail(GetOrExpireTokenRequestCommon request)
        {
            var parameters = new DynamicParameters(new
            {
                request.roleType
            });
            parameters.Add("@userId", request.actionUserId);

            string proc = "apiproc_get_cognito_token_detail";
            var dbResponse = await _dao.ExecuteAsync<TokenDetailsCommon>(proc, parameters);
            return dbResponse;
        }

        public async Task<CommonDBResponse> ManageToken(ManageTokenRequestCommon request)
        {
            var parameters = new DynamicParameters(new
            {
                request.roleType,
                request.accessToken,
                request.accessTokenExpiryTimeMS,
                request.refreshToken,
                request.refreshTokenExpiryTimeMS
            });

            parameters.Add("@userId", request.actionUserId);

            string proc = "apiproc_manage_cognito_token";
            var dbResponse = await _dao.ExecuteAsync<CommonDBResponse>(proc, parameters);
            return dbResponse;
        }

        public async Task<CommonDBResponse> ExpireToken(GetOrExpireTokenRequestCommon request)
        {
            var parameters = new DynamicParameters(new
            {
                request.roleType,
                request.actionIP,
                request.actionPlatform
            });
            parameters.Add("@userId", request.actionUserId);
            parameters.Add("@actionUser", request.actionUserId);

            string proc = "apiproc_expire_cognito_token";
            var dbResponse = await _dao.ExecuteAsync<CommonDBResponse>(proc, parameters);
            return dbResponse;
        }

        public async Task<AuthDbResponse> CreateOtpAsync(
            string username,
            string purpose,
            string otpHash,
            string otpCode,
            string traceId,
            string cognitoAuthSession
        )
        {
            string proc = "dbo.apiproc_cp_auth_create_otp";
            var parameters = new DynamicParameters(new
            {
                username,
                purpose,
                otpHash,
                otpCode,
                traceId,
                cognitoAuthSession
            });

            return await _dao.ExecuteAsync<AuthDbResponse>(proc, parameters);
        }

        public async Task<AuthDbResponse> VerifyOtpAsync(
            string username,
            string purpose,
            string otpCode,
            string submittedOtpHash = ""
        )
        {
            string proc = "dbo.apiproc_cp_auth_verify_otp";
            var parameters = new DynamicParameters(new
            {
                username,
                purpose,
                submittedOtpHash,
                otpCode
            });

            return await _sqlTransactionHandler.ExecuteWithTransactionAsync<AuthDbResponse>(proc, parameters);
        }

        public async Task<AuthDbResponse> VerifyOtpWithOutTransactionAsync(
            string username,
            string purpose,
            string otpCode,
            string submittedOtpHash = ""
        )
        {
            string proc = "dbo.apiproc_cp_auth_verify_otp";
            var parameters = new DynamicParameters(new
            {
                username,
                purpose,
                submittedOtpHash,
                otpCode
            });

            return await _dao.ExecuteAsync<AuthDbResponse>(proc, parameters);
        }
    }
}