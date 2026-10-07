using AmazonCognito.Middleware.Data;
using AmazonCognito.Middleware.Domain.Abstract;
using hoslog.customer.common.Service;
using AmazonCognitoMiddlewareData = AmazonCognito.Middleware.Data;

namespace hoslog.customer.common.Middleware.AmazonCognito
{
    public class AmazonCognitoMiddleware
    {
        private readonly CognitoMiddlewareAbstract _cognitoMiddlewareAbstract;
        private readonly ApiResponseMessageService _apiResponseMessageService;
        public AmazonCognitoMiddleware(CognitoMiddlewareAbstract cognitoMiddlewareAbstract, ApiResponseMessageService apiResponseMessageService)
        {
            _cognitoMiddlewareAbstract = cognitoMiddlewareAbstract;
            _apiResponseMessageService = apiResponseMessageService;
        }

        #region Sign Up
        public async Task<Models.Middleware.AmazonCognito.CommonResponse> SignUpAsync(Models.Middleware.AmazonCognitoModel.SignUp.SignUpModel.Request request)
        {
            var signUpRequest = new AmazonCognitoMiddlewareData.SignUp.SignUpModel.Request
            {
                Username = request.Username,
                Password = request.Password,
                AttributeType = request.AttributeType.MapObjects<AmazonCognitoMiddlewareData.SignUp.SignUpModel.AttributeType>()
            };
            var signUpResponse = await _cognitoMiddlewareAbstract.SignUpAsync(signUpRequest);
            var mappedResponse = signUpResponse?.MapObject<Models.Middleware.AmazonCognito.CommonResponse>();
            return await HandleMiddlewareResponse(mappedResponse);
        }

        public async Task<Models.Middleware.AmazonCognito.CommonResponse> AdminCreateUserAsync(Models.Middleware.AmazonCognitoModel.SignUp.SignUpModel.Request request)
        {
            var signUpRequest = new AmazonCognitoMiddlewareData.SignUp.SignUpModel.Request
            {
                Username = request.Username,
                Password = request.Password,
                AttributeType = request.AttributeType.MapObjects<AmazonCognitoMiddlewareData.SignUp.SignUpModel.AttributeType>()
            };
            var signUpResponse = await _cognitoMiddlewareAbstract.AdminCreateUserAsync(signUpRequest);
            var mappedResponse = signUpResponse?.MapObject<Models.Middleware.AmazonCognito.CommonResponse>();
            return await HandleMiddlewareResponse(mappedResponse);
        }

        public async Task<Models.Middleware.AmazonCognito.CommonResponse> ResendSignUpConfirmationCodeAsync(Models.Middleware.AmazonCognitoModel.SignUp.ResendSignUpConfirmationCodeModel.Request request)
        {
            var resendSignUpConfirmationCodeRequest = request.MapObject<AmazonCognitoMiddlewareData.SignUp.ResendSignUpConfirmationCodeModel.Request>();
            var resendSignUpConfirmationCodeRequestResponse = await _cognitoMiddlewareAbstract.ResendSignUpConfirmationCodeAsync(resendSignUpConfirmationCodeRequest);
            var mappedResponse = resendSignUpConfirmationCodeRequestResponse?.MapObject<Models.Middleware.AmazonCognito.CommonResponse>();
            return await HandleMiddlewareResponse(mappedResponse);
        }

        public async Task<Models.Middleware.AmazonCognito.CommonResponse> ConfirmSignUpAsync(Models.Middleware.AmazonCognitoModel.SignUp.ConfirmSignUpModel.Request request)
        {
            var confirmSignUpRequest = request.MapObject<AmazonCognitoMiddlewareData.SignUp.ConfirmSignUpModel.Request>();
            var confirmSignUpResponse = await _cognitoMiddlewareAbstract.ConfirmSignUpAsync(confirmSignUpRequest);
            var mappedResponse = confirmSignUpResponse?.MapObject<Models.Middleware.AmazonCognito.CommonResponse>();
            return await HandleMiddlewareResponse(mappedResponse);
        }
        #endregion

        #region Auth
        public async Task<Models.Middleware.AmazonCognito.CommonResponse> SignInAsync(Models.Middleware.AmazonCognitoModel.Auth.SignInModel.Request request)
        {
            var signInRequest = request.MapObject<AmazonCognitoMiddlewareData.Auth.SignInModel.Request>();
            var signInResponse = await _cognitoMiddlewareAbstract.SignInAsync(signInRequest);
            var mappedResponse = signInResponse?.MapObject<Models.Middleware.AmazonCognito.CommonResponse>();
            return await HandleMiddlewareResponse(mappedResponse);
        }

        public async Task<Models.Middleware.AmazonCognito.CommonResponse> RefreshAsync(Models.Middleware.AmazonCognitoModel.Auth.RefreshModel.Request request)
        {
            var refreshRequest = request.MapObject<AmazonCognitoMiddlewareData.Auth.RefreshModel.Request>();
            var refreshResponse = await _cognitoMiddlewareAbstract.RefreshAsync(refreshRequest);
            var mappedResponse = refreshResponse?.MapObject<Models.Middleware.AmazonCognito.CommonResponse>();
            return await HandleMiddlewareResponse(mappedResponse);
        }

        public async Task<Models.Middleware.AmazonCognito.CommonResponse> SignOutAsync(Models.Middleware.AmazonCognitoModel.Auth.SignOutModel.Request request)
        {
            var signOutRequest = request.MapObject<AmazonCognitoMiddlewareData.Auth.SignOutModel.Request>();
            var signOutResponse = await _cognitoMiddlewareAbstract.SignOutAsync(signOutRequest);
            var mappedResponse = signOutResponse?.MapObject<Models.Middleware.AmazonCognito.CommonResponse>();
            return await HandleMiddlewareResponse(mappedResponse);
        }

        public async Task<Models.Middleware.AmazonCognito.CommonResponse> AdminUserGlobalSignOutAsync(string userName)
        {
            var signOutResponse = await _cognitoMiddlewareAbstract.AdminUserGlobalSignOutAsync(userName);
            var mappedResponse = signOutResponse?.MapObject<Models.Middleware.AmazonCognito.CommonResponse>();
            return await HandleMiddlewareResponse(mappedResponse);
        }

        public async Task<bool> IsValidTokenAsync(string accessToken)
        {
            return await _cognitoMiddlewareAbstract.IsValidTokenAsync(accessToken);
        }
        #endregion

        #region Password
        public async Task<Models.Middleware.AmazonCognito.CommonResponse> ChangePasswordAsync(Models.Middleware.AmazonCognitoModel.Password.ChangePasswordModel.Request request)
        {
            var changePasswordRequest = request.MapObject<AmazonCognitoMiddlewareData.Password.ChangePasswordModel.Request>();
            var changePasswordResponse = await _cognitoMiddlewareAbstract.ChangePasswordAsync(changePasswordRequest);
            var mappedResponse = changePasswordResponse?.MapObject<Models.Middleware.AmazonCognito.CommonResponse>();
            return await HandleMiddlewareResponse(mappedResponse);
        }

        public async Task<Models.Middleware.AmazonCognito.CommonResponse> ForgotPasswordAsync(Models.Middleware.AmazonCognitoModel.Password.ForgotPasswordModel.Request request)
        {
            var forgotPasswordRequest = request.MapObject<AmazonCognitoMiddlewareData.Password.ForgotPasswordModel.Request>();
            var forgotPasswordResponse = await _cognitoMiddlewareAbstract.ForgotPasswordAsync(forgotPasswordRequest);
            var mappedResponse = forgotPasswordResponse?.MapObject<Models.Middleware.AmazonCognito.CommonResponse>();
            return await HandleMiddlewareResponse(mappedResponse);
        }

        public async Task<Models.Middleware.AmazonCognito.CommonResponse> ResendForgotPasswordConfirmationCodeAsync(Models.Middleware.AmazonCognitoModel.Password.ResendForgotPasswordConfirmationCodeModel.Request request)
        {
            var resendForgotPasswordConfirmationCodeRequest = request.MapObject<AmazonCognitoMiddlewareData.Password.ResendForgotPasswordConfirmationCodeModel.Request>();
            var resendForgotPasswordConfirmationCodeResponse = await _cognitoMiddlewareAbstract.ResendForgotPasswordConfirmationCodeAsync(resendForgotPasswordConfirmationCodeRequest);
            var mappedResponse = resendForgotPasswordConfirmationCodeResponse?.MapObject<Models.Middleware.AmazonCognito.CommonResponse>();
            return await HandleMiddlewareResponse(mappedResponse);
        }

        public async Task<Models.Middleware.AmazonCognito.CommonResponse> ConfirmForgotPasswordAsync(Models.Middleware.AmazonCognitoModel.Password.ConfirmForgotPasswordModel.Request request)
        {
            var confirmForgotPasswordRequest = request.MapObject<AmazonCognitoMiddlewareData.Password.ConfirmForgotPasswordModel.Request>();
            var confirmForgotPasswordResponse = await _cognitoMiddlewareAbstract.ConfirmForgotPasswordAsync(confirmForgotPasswordRequest);
            var mappedResponse = confirmForgotPasswordResponse?.MapObject<Models.Middleware.AmazonCognito.CommonResponse>();
            return await HandleMiddlewareResponse(mappedResponse);
        }

        public async Task<Models.Middleware.AmazonCognito.CommonResponse> SetPasswordAsync(Models.Middleware.AmazonCognitoModel.Password.SetPasswordModel.Request request)
        {
            var setPasswordRequest = request.MapObject<AmazonCognitoMiddlewareData.Password.SetPasswordModel.Request>();
            var setPasswordResponse = await _cognitoMiddlewareAbstract.SetPasswordAsync(setPasswordRequest);
            var mappedResponse = setPasswordResponse?.MapObject<Models.Middleware.AmazonCognito.CommonResponse>();
            return await HandleMiddlewareResponse(mappedResponse);
        }
        #endregion

        #region User management
        public async Task<Models.Middleware.AmazonCognito.CommonResponse> DeleteAccountAsync(Models.Middleware.AmazonCognitoModel.UserManagement.DeleteAccountModel.Request request)
        {
            var deleteAccountRequest = request.MapObject<AmazonCognitoMiddlewareData.UserManagement.DeleteAccountModel.Request>();
            var deleteAccountResponse = await _cognitoMiddlewareAbstract.DeleteAccountAsync(deleteAccountRequest);
            var mappedResponse = deleteAccountResponse?.MapObject<Models.Middleware.AmazonCognito.CommonResponse>();
            return await HandleMiddlewareResponse(mappedResponse);
        }

        public async Task<Models.Middleware.AmazonCognito.CommonResponse> AdminDeleteAccountAsync(Models.Middleware.AmazonCognitoModel.UserManagement.AdminDeleteAccountModel.Request request)
        {
            var adminDeleteAccountRequest = request.MapObject<AmazonCognitoMiddlewareData.UserManagement.AdminDeleteAccountModel.Request>();
            var adminDeleteAccountResponse = await _cognitoMiddlewareAbstract.AdminDeleteAccountAsync(adminDeleteAccountRequest);
            var mappedResponse = adminDeleteAccountResponse?.MapObject<Models.Middleware.AmazonCognito.CommonResponse>();
            return await HandleMiddlewareResponse(mappedResponse);
        }

        public async Task<Models.Middleware.AmazonCognito.CommonResponse> UpdateUserAttributesAsync(Models.Middleware.AmazonCognitoModel.UserManagement.UpdateUserAttributesModel.Request request)
        {
            var updateUserAttributesRequest = request.MapObject<AmazonCognitoMiddlewareData.UserManagement.UpdateUserAttributesModel.Request>();
            var updateUserAttributesResponse = await _cognitoMiddlewareAbstract.UpdateUserAttributesAsync(updateUserAttributesRequest);
            var mappedResponse = updateUserAttributesResponse?.MapObject<Models.Middleware.AmazonCognito.CommonResponse>();
            return await HandleMiddlewareResponse(mappedResponse);
        }

        public async Task<Models.Middleware.AmazonCognito.CommonResponse> MarkEmailAsVerifiedAsync(Models.Middleware.AmazonCognitoModel.UserManagement.MarkEmailAsVerifiedModel.Request request)
        {
            var markEmailAsVerifiedRequest = request.MapObject<AmazonCognitoMiddlewareData.UserManagement.MarkEmailAsVerifiedModel.Request>();
            var markEmailAsVerifiedResponse = await _cognitoMiddlewareAbstract.MarkEmailAsVerifiedAsync(markEmailAsVerifiedRequest);
            var mappedResponse = markEmailAsVerifiedResponse?.MapObject<Models.Middleware.AmazonCognito.CommonResponse>();
            return await HandleMiddlewareResponse(mappedResponse);
        }

         public async Task<Models.Middleware.AmazonCognito.CommonResponse> MarkMobileNumberAsVerifiedAsync(Models.Middleware.AmazonCognitoModel.UserManagement.MarkMobileNumberAsVerifiedModel.Request request)
        {
            var markEmailAsVerifiedRequest = request.MapObject<AmazonCognitoMiddlewareData.UserManagement.MarkMobileNumberAsVerifiedModel.Request>();
            var markEmailAsVerifiedResponse = await _cognitoMiddlewareAbstract.MarkMobileNumberAsVerifiedAsync(markEmailAsVerifiedRequest);
            var mappedResponse = markEmailAsVerifiedResponse?.MapObject<Models.Middleware.AmazonCognito.CommonResponse>();
            return await HandleMiddlewareResponse(mappedResponse);
        }

        public async Task<Models.Middleware.AmazonCognito.CommonResponse> GetUserDetailAsync(Models.Middleware.AmazonCognitoModel.UserManagement.UserDetailModel.Request request)
        {
            var getUserDetailRequest = request.MapObject<AmazonCognitoMiddlewareData.UserManagement.UserDetailModel.Request>();
            var getUserDetailResponse = await _cognitoMiddlewareAbstract.GetUserDetailAsync(getUserDetailRequest);
            var mappedResponse = getUserDetailResponse?.MapObject<Models.Middleware.AmazonCognito.CommonResponse>();
            return await HandleMiddlewareResponse(mappedResponse);
        }

        public async Task<Models.Middleware.AmazonCognito.CommonResponse> CheckAttributeExistsWithDetailsAsync(Models.Middleware.AmazonCognitoModel.UserManagement.CheckAttributeExistsWithDetailsModel.Request request)
        {
            var checkAttributeExistsWithDetailsRequest = request.MapObject<AmazonCognitoMiddlewareData.UserManagement.CheckAttributeExistsWithDetailsModel.Request>();
            var checkAttributeExistsWithDetailsResponse = await _cognitoMiddlewareAbstract.CheckAttributeExistsWithDetailsAsync(checkAttributeExistsWithDetailsRequest);
            var mappedResponse = checkAttributeExistsWithDetailsResponse?.MapObject<Models.Middleware.AmazonCognito.CommonResponse>();
            return await HandleMiddlewareResponse(mappedResponse);
        }
        #endregion

        public async Task<Models.Middleware.AmazonCognitoModel.UserManagement.CheckAttributeExistsWithDetailsModel.Response> CheckUserExistsAsync(string mobileNumber = "", string email = "")
        {
            try
            {
                async Task<Models.Middleware.AmazonCognitoModel.UserManagement.CheckAttributeExistsWithDetailsModel.Response> CheckAttributeAsync(string attributeName, string attributeValue)
                {
                    var request = new Models.Middleware.AmazonCognitoModel.UserManagement.CheckAttributeExistsWithDetailsModel.Request
                    {
                        attributeName = attributeName,
                        attributeValue = attributeValue
                    };
                    var response = await CheckAttributeExistsWithDetailsAsync(request);
                    if (response != null && response.Code == Models.Middleware.AmazonCognito.ResponseCode.Success)
                    {
                        return response?.Data?.MapObject<Models.Middleware.AmazonCognitoModel.UserManagement.CheckAttributeExistsWithDetailsModel.Response>();
                    }
                    return null;
                }

                // Check by email
                if (!string.IsNullOrEmpty(email))
                {
                    var emailExists = await CheckAttributeAsync("email", email);
                    if (emailExists != null && emailExists.Users.Count > 0) return emailExists;
                }

                // Check by mobile number
                if (!string.IsNullOrEmpty(mobileNumber))
                {
                    var phoneExists = await CheckAttributeAsync("phone_number", mobileNumber);
                    if (phoneExists != null && phoneExists.Users.Count > 0) return phoneExists;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log exception here (e.g., using a logger)
                return null;
            }
        }

        private async Task<Models.Middleware.AmazonCognito.CommonResponse> HandleMiddlewareResponse(Models.Middleware.AmazonCognito.CommonResponse response)
        {
            if (response == null)
            {
                string message = Models.Middleware.AmazonCognito.ResponseCode.Exception.ToString();
                return new Models.Middleware.AmazonCognito.CommonResponse
                {
                    Code = Models.Middleware.AmazonCognito.ResponseCode.Exception,
                    Message = message,
                    Errors = new List<Models.Middleware.AmazonCognito.Error>
                    {
                        new Models.Middleware.AmazonCognito.Error
                        {
                            ErrorMessage = message
                        }
                    }
                };
            }
            else if (response.Code == Models.Middleware.AmazonCognito.ResponseCode.Exception)
            {
                string message = await _apiResponseMessageService.GetMessageByCodeAsync(response?.Message);
                return new Models.Middleware.AmazonCognito.CommonResponse
                {
                    Code = Models.Middleware.AmazonCognito.ResponseCode.Exception,
                    Message = message,
                    Errors = new List<Models.Middleware.AmazonCognito.Error>
                    {
                        new Models.Middleware.AmazonCognito.Error
                        {
                            ErrorMessage = message
                        }
                    }
                };
            }

            return response;
        }
    }
}
