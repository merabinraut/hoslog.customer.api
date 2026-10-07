using Dapper;
using hoslog.customer.api.Models.UserRegistration;
using hoslog.customer.common.Models.Middleware.AmazonCognito;
using hoslog.customer.common.Utilities;
using hoslog.customer.shared.UserRegistration;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.domain.UserRegistration
{
    public class UserRegistrationRepository : IUserRegistrationRepository
    {
        private readonly RepositoryDao _dao;
        private readonly SqlTransactionHandler _sqlTransactionHandler;
        public UserRegistrationRepository(IConfiguration configuration, SqlTransactionHandler sqlTransactionHandler)
        {
            _dao = new RepositoryDao(configuration);
            _sqlTransactionHandler = sqlTransactionHandler;
        }

        public async Task<ChangeCustomerPasswordResponseCommon> ChangeCustomerPasswordAsync(CreatePasswordRequestCommon mappedRequest)
        {
            var parameters = new DynamicParameters();
            string proc = "apiproc_customer_change_password ";
            parameters.Add("@customerId", mappedRequest.actionUserId);
            parameters.Add("@newPassword", mappedRequest.newPassword);
            parameters.Add("@currentPassword", mappedRequest.oldPassword);
            var dbResponse = await _sqlTransactionHandler.ExecuteWithTransactionAsync<ChangeCustomerPasswordResponseCommon>(proc, parameters);
            return dbResponse;
        }

        public async Task<ForgotPasswordVerificationResponseCommon> ForgotPasswordOTPVerificationAsync(ForgotPasswordOTPVerificatonRequestModel mappedRequest)
        {

            var parameters = new DynamicParameters();
            string proc = "apiproc_customer_forgot_password_OTP_Verification_details ";
            parameters.Add("@mobileNumber", mappedRequest.mobileNumber);
            parameters.Add("@agentId", mappedRequest.customerId);
            parameters.Add("@verificationCode", mappedRequest.otpCode);
            parameters.Add("@processId", mappedRequest.processId);
            var dbResponse = await _sqlTransactionHandler.ExecuteWithTransactionAsync<ForgotPasswordVerificationResponseCommon>(proc, parameters);
            return dbResponse;
        }

        public async Task<ForgotPasswordResendResponseCommon> ForgotPasswordResendOTPAsync(ForgotPasswordResendOTPModel mappedRequest)
        {
            var parameters = new DynamicParameters();
            string proc = "apiproc_customer_Forgot_password_resend_otp";
            parameters.Add("@mobileNumber", mappedRequest.mobileNumber);
            parameters.Add("@customerId", mappedRequest.customerId);
            var dbResponse = await _dao.ExecuteAsync<ForgotPasswordResendResponseCommon>(proc, parameters);
            return dbResponse;
        }

        public async Task<ForgotSetPasswordResponseCommon> ForgotPasswordSetOTPAsync(ForgotSetPasswordCommon request)
        {
            var parameters = new DynamicParameters();
            string proc = "apiproc_customer_forgot_set_password";
            parameters.Add("@customerId", request.customerId);
            parameters.Add("@password", request.password);
            parameters.Add("@processId", request.processId);
            var dbResponse = await _sqlTransactionHandler.ExecuteWithTransactionAsync<ForgotSetPasswordResponseCommon>(proc, parameters);
            return dbResponse;
        }

        public async Task<ForgotPasswordResponseCommon> ForgotUserPasswordAsync(ForgotPasswordRequestModel mappedRequest)
        {
            var parameters = new DynamicParameters();
            string proc = "apiproc_customer_forgot_password";
            parameters.Add("@mobileNumber", mappedRequest.mobileNumber);
            var dbResponse = await _sqlTransactionHandler.ExecuteWithTransactionAsync<ForgotPasswordResponseCommon>(proc, parameters);
            return dbResponse;
        }

        public async Task<CreatePasswordResponseCommon> UserCreatePasswordAsync(CreatePasswordModel mappedRequest)
        {
            var parameters = new DynamicParameters();
            string proc = "apiproc_customer_create_password";
            parameters.Add("@customerId", mappedRequest.customerId);
            parameters.Add("@mobileNumber", mappedRequest.mobileNumber);
            parameters.Add("@password", mappedRequest.password);
            parameters.Add("@processId", mappedRequest.processId);
            var dbResponse = await _sqlTransactionHandler.ExecuteWithTransactionAsync<CreatePasswordResponseCommon>(proc, parameters);
            return dbResponse;
        }

        public async Task<UserRegistrationResponseCommon> UserRegistrationAsync(UserRegistrationCommon mappedRequest)
        {
            var parameters = new DynamicParameters();
            string proc = "apiproc_customer_registration";
            parameters.Add("@nickName", mappedRequest.nickName);
            parameters.Add("@mobileNumber", mappedRequest.mobileNumber);
            parameters.Add("@referCode", mappedRequest.referCode);
            parameters.Add("@referType", mappedRequest.referType);
            parameters.Add("@dob", mappedRequest.dob);
            parameters.Add("@email", mappedRequest.email);
            parameters.Add("@actionUser", mappedRequest.actionUserId);
            parameters.Add("@actionIP", mappedRequest.actionIP);
            parameters.Add("@actionPlatform", mappedRequest.actionPlatform);
            var dbResponse = await _sqlTransactionHandler.ExecuteWithTransactionAsync<UserRegistrationResponseCommon>(proc, parameters);
            return dbResponse;
        }

        public async Task<UserRegistrationVerificationResponse> UserRegistrationVerificationOTP(UserRegistrationVerificationRequestCommon mappedRequest)
        {
            var parameters = new DynamicParameters();
            string proc = "apiproc_customer_registration_verification_OTP";
            parameters.Add("@holdCustomerId", mappedRequest.holdCustomerId);
            parameters.Add("@verificationCode", mappedRequest.otpCode);
            parameters.Add("@mobileNumber", mappedRequest.mobileNumber);
            parameters.Add("@actionUser", mappedRequest.mobileNumber);
            parameters.Add("@processId", mappedRequest.processId);
            parameters.Add("@cognitoUserId", mappedRequest.cognitoUserId);
            parameters.Add("@actionUser", mappedRequest.actionUserId);
            parameters.Add("@actionIP", mappedRequest.actionIP);
            parameters.Add("@actionPlatform", mappedRequest.actionPlatform);
            var dbResponse = await _sqlTransactionHandler.ExecuteWithTransactionAsync<UserRegistrationVerificationResponse>(proc, parameters);
            return dbResponse;
        }

        public async Task<ResendOTPResponsecommon> UserResendOTPAsync(ResendOTPRequestCommon mappedRequest)
        {
            var parameters = new DynamicParameters();
            string proc = "apiproc_customer_resend_otp_verification_request";
            parameters.Add("@holdCustomerId", mappedRequest.holdCustomerId);
            parameters.Add("@mobileNumber", mappedRequest.mobileNumber);
            parameters.Add("@actionUser", mappedRequest.actionUserId);
            parameters.Add("@actionIP", mappedRequest.actionIP);
            parameters.Add("@actionPlatform", mappedRequest.actionPlatform);
            var dbResponse = await _sqlTransactionHandler.ExecuteWithTransactionAsync<ResendOTPResponsecommon>(proc, parameters);
            return dbResponse;
        }

        public async Task<HoldCustomerDetailCommon> HoldCustomerDetail(string mobileNumber, string holdCustomerId, string processId = "", string processType = "", string expiryTime = "")
        {
            var parameters = new DynamicParameters(new
            {
                mobileNumber,
                holdCustomerId,
                processId,
                processType,
                expiryTime
            });
            string proc = "apiproc_cp_get_hold_customer_detail";
            var response = await _dao.ExecuteAsync<HoldCustomerDetailCommon>(proc, parameters);
            return response;
        }

        #region Remove Unconfirmed Customer Information
        public async Task<CommonResponse> RemoveUnconfirmedCustomerInformation(RemoveUnconfirmedCustomerInformationCommon request)
        {
            var parameters = new DynamicParameters();
            string proc = "apiproc_cp_remove_unconfirmed_customer_information";
            parameters.Add("@nickName", request.nickName);
            parameters.Add("@exceptUserSno", request.exceptUserSno);
            parameters.Add("@actionUser", request.actionUserId);
            parameters.Add("@actionIP", request.actionIP);
            parameters.Add("@actionPlatform", request.actionPlatform);
            return await _sqlTransactionHandler.ExecuteWithTransactionAsync<CommonResponse>(proc, parameters);
        }
        #endregion
    }
}
