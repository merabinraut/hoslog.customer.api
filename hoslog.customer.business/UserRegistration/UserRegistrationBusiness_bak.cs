// using System.Diagnostics;
// using hoslog.customer.api.Models.UserRegistration;
// using hoslog.customer.business.User;
// using hoslog.customer.common;
// using hoslog.customer.common.Middleware.AmazonCognito;
// using hoslog.customer.common.Models.Auth;
// using hoslog.customer.common.Models.Middleware.AmazonCognito;
// using hoslog.customer.common.Models.Middleware.AmazonCognitoModel.Auth;
// using hoslog.customer.common.Models.Middleware.AmazonCognitoModel.Password;
// using hoslog.customer.common.Models.Middleware.AmazonCognitoModel.SignUp;
// using hoslog.customer.common.Models.Middleware.AmazonCognitoModel.UserManagement;
// using hoslog.customer.common.Service;
// using hoslog.customer.common.Utilities;
// using hoslog.customer.domain.UserRegistration;
// using hoslog.customer.shared;
// using hoslog.customer.shared.UserRegistration;
// using Microsoft.AspNetCore.Http;
// using Microsoft.Extensions.Configuration;
// using System.Net;
// using System.Text.RegularExpressions;
// using hoslog.customer.business.Conversion;
// using hoslog.customer.common.Models.Conversion;
// using hoslog.customer.domain.Conversion;
// using hoslog.customer.shared.Conversion;
// using Microsoft.Extensions.Logging;
// using Newtonsoft.Json;

// namespace hoslog.customer.business.UserRegistration
// {
//     public class UserRegistrationBusiness_bak //: IUserRegistrationBusiness
//     {
//         private readonly ILogger<UserRegistrationBusiness> _logger;
//         private readonly IConversionRepository _conversion;
//         private readonly IUserRegistrationRepository _userRegistrationRepository;
//         private readonly CommonResponseHandler _commonResponseHandler;
//         private readonly ValidationHelper _validationHelper;
//         private readonly IHttpContextAccessor _httpContextAccessor;
//         private readonly StringCipher _stringCipher;
//         private readonly AmazonCognitoMiddleware _amazonCognitoMiddleware;
//         private readonly IConfiguration _configuration;
//         private readonly IUserBusiness _userBusiness;
//         private readonly SqlTransactionHandler _sqlTransactionHandler;
//         private readonly ApiResponseMessageService _apiResponseMessageService;
//         public UserRegistrationBusiness_bak(IConversionRepository conversion, ILogger<UserRegistrationBusiness> logger, IUserRegistrationRepository userRegistrationRepository, ValidationHelper validationHelper,
//             IHttpContextAccessor httpContextAccessor, StringCipher stringCipher, AmazonCognitoMiddleware amazonCognitoMiddleware,
//             IConfiguration configuration, IUserBusiness userBusiness, SqlTransactionHandler sqlTransactionHandler, CommonResponseHandler commonResponseHandler,
//             ApiResponseMessageService apiResponseMessageService
//         )
//         {
//             _conversion = conversion;
//             _logger = logger;
//             _userRegistrationRepository = userRegistrationRepository;
//             _commonResponseHandler = commonResponseHandler;
//             _validationHelper = validationHelper;
//             _httpContextAccessor = httpContextAccessor;
//             _stringCipher = stringCipher;
//             _amazonCognitoMiddleware = amazonCognitoMiddleware;
//             _configuration = configuration;
//             _userBusiness = userBusiness;
//             _sqlTransactionHandler = sqlTransactionHandler;
//             _apiResponseMessageService = apiResponseMessageService;
//         }
//         public async Task<(HttpStatusCode, object)> UserRegistrationAsync(UserRegistrationModel mappedRequest, ConversionSessionContext session)
//         {
//             var validationResult = await _validationHelper.ValidateModelAndSignatureAsync(mappedRequest, req => new UserRegistrationModelValidator().Validate(req));
//             if (validationResult.Item1 != HttpStatusCode.OK)
//                 return validationResult;
//             var resp = new CommonApiResponse();
//             if (mappedRequest.isCheckedTermsAndCondition != true)
//             {
//                 resp.code = ResponseCodes.Code400_BadRequest;
//                 resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                 resp.data = Array.Empty<string>();
//                 resp.errors = new List<Errors>
//                 {
//                     new Errors
//                     {
//                         errorCode = ResponseCodes.CUEREG100_RegistrationMustAgreeTermsAndCondition,
//                         errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUEREG100_RegistrationMustAgreeTermsAndCondition)
//                     }
//                 };
//                 return (HttpStatusCode.BadRequest, resp);
//             }

//             if (mappedRequest.isCheckedTermsAndCondition == false)
//                 return await _commonResponseHandler.HandleBadRequestResponse<CommonApiResponse>();

//             var pattern = @"^(?:\d{4}[-/]{2}|\d{4}[-/]{2}\d{1,2}|\d{4}[-/]\d{1,2}[-/]{2}|[-/]{2}\d{1,2}|-?\d{1,2}[-/]\d{1,2}|\d{4}[-/]\d{1,2}[-/]\d{1,2}|-?\d{1,2}[-/]|[-/]{2})$";

//             if (!string.IsNullOrWhiteSpace(mappedRequest?.dob) &&
//                 mappedRequest.dob.Trim() != "--" &&
//                 !Regex.IsMatch(mappedRequest.dob.Trim(), pattern))
//             {
//                 resp.code = ResponseCodes.Code400_BadRequest;
//                 resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUE120_InvalidDateFormat.ToString());
//                 resp.data = Array.Empty<string>();
//                 resp.errors = new List<Errors>
//                         {
//                             new Errors { errorCode = ResponseCodes.CUE120_InvalidDateFormat,errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUE120_InvalidDateFormat.ToString()) }
//                         };
//                 return (HttpStatusCode.BadRequest, resp);
//             }
//             mappedRequest.dob = NormalizeDob(mappedRequest?.dob);
//             _sqlTransactionHandler.BeginTransaction();
//             try
//             {
//                 var dbResponse = await _userRegistrationRepository.UserRegistrationAsync(mappedRequest);

//                 if (dbResponse.code != "0")
//                 {
//                     await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
//                         eventName: "registration_failed",
//                         stepCode: "registration",
//                         eventStatus: "identity",
//                         relatedEntityType: "registration",
//                         errorCode: dbResponse?.code ?? "",
//                         errorMessage: dbResponse?.message ?? "",
//                         properties: mappedRequest
//                       );

//                     resp.code = ResponseCodes.Code400_BadRequest;
//                     resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                     resp.errors = new List<Errors>
//                     {
//                         new Errors
//                         {
//                             errorCode = dbResponse.message,
//                             errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse.message)
//                         }
//                     };
//                     _sqlTransactionHandler.RollbackTransaction();
//                     return (HttpStatusCode.BadRequest, resp);
//                 }

//                 var countryCode = _configuration["CountryCode"];

//                 var formattedMobileNumber = FormatPhoneNumber(mappedRequest?.mobileNumber, countryCode);
//                 var checkUserExistsResponse = await _amazonCognitoMiddleware.CheckUserExistsAsync(formattedMobileNumber);
//                 var checkUserExistsResponseObject = checkUserExistsResponse?.MapObject<common.Models.Middleware.AmazonCognitoModel.UserManagement.CheckAttributeExistsWithDetailsModel.Response>()
//                                                     ?? new common.Models.Middleware.AmazonCognitoModel.UserManagement.CheckAttributeExistsWithDetailsModel.Response();

//                 async Task<bool> HandleUnconfirmedUserAsync(string username, string actionPlatform)
//                 {
//                     var adminDeleteResponse = await _amazonCognitoMiddleware.AdminDeleteAccountAsync(new common.Models.Middleware.AmazonCognitoModel.UserManagement.AdminDeleteAccountModel.Request
//                     {
//                         userName = username
//                     });

//                     if (adminDeleteResponse == null || adminDeleteResponse?.Code != hoslog.customer.common.Models.Middleware.AmazonCognito.ResponseCode.Success)
//                     {
//                         resp.code = ResponseCodes.Code400_BadRequest;
//                         resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                         resp.errors = new List<Errors>
//                         {
//                             new Errors
//                             {
//                                 errorCode = ResponseCodes.CUEREG101_DuplicateMobileNumberOrNickname,
//                                 errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUEREG101_DuplicateMobileNumberOrNickname)
//                             }
//                         };
//                         _sqlTransactionHandler.RollbackTransaction();
//                         return false;
//                     }

//                     var removeUnconfirmedCustomerInformationResponse = await _userRegistrationRepository.RemoveUnconfirmedCustomerInformation(new RemoveUnconfirmedCustomerInformationCommon
//                     {
//                         nickName = username,
//                         actionIP = _httpContextAccessor?.HttpContext?.GetIpAddress(),
//                         actionPlatform = actionPlatform
//                     });

//                     if (removeUnconfirmedCustomerInformationResponse.Code != ResponseCode.Success)
//                     {
//                         resp.code = ResponseCodes.Code400_BadRequest;
//                         resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                         resp.errors = new List<Errors>
//                         {
//                             new Errors
//                             {
//                                 errorCode = ResponseCodes.CUEREG101_DuplicateMobileNumberOrNickname,
//                                 errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUEREG101_DuplicateMobileNumberOrNickname)
//                             }
//                         };
//                         _sqlTransactionHandler.RollbackTransaction();
//                         return false;
//                     }

//                     return true;
//                 }

//                 // Handle each user in checkUserExistsResponseObject
//                 foreach (var user in checkUserExistsResponseObject?.Users)
//                 {
//                     if (string.Equals(user?.UserStatus, "UNCONFIRMED", StringComparison.OrdinalIgnoreCase))
//                     {
//                         if (!await HandleUnconfirmedUserAsync(user.Username, "CustomerAPI"))
//                         {
//                             return (HttpStatusCode.BadRequest, resp);
//                         }
//                     }
//                     else
//                     {
//                         await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
//                             eventName: "registration_failed",
//                             stepCode: "identity",
//                             eventStatus: "failure",
//                             relatedEntityType: "registration",
//                             errorCode: ResponseCodes.CUEREG101_DuplicateMobileNumberOrNickname,
//                             errorMessage: await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUEREG101_DuplicateMobileNumberOrNickname),
//                             properties: mappedRequest
//                         );

//                         resp.code = ResponseCodes.Code400_BadRequest;
//                         resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                         resp.errors = new List<Errors>
//                         {
//                             new Errors
//                             {
//                                 errorCode = ResponseCodes.CUEREG101_DuplicateMobileNumberOrNickname,
//                                 errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUEREG101_DuplicateMobileNumberOrNickname)
//                             }
//                         };
//                         _sqlTransactionHandler.RollbackTransaction();
//                         return (HttpStatusCode.BadRequest, resp);
//                     }
//                 }

//                 // // Handle the current user details
//                 // var getUserDetailResponse = await _amazonCognitoMiddleware.GetUserDetailAsync(new UserDetailModel.Request
//                 // {
//                 //     Username = dbResponse?.cognitoUserName
//                 // });

//                 // if (getUserDetailResponse?.Code == ResponseCode.Success)
//                 // {
//                 //     var getUserDetailResponseObject = getUserDetailResponse?.Data?.MapObject<UserDetailModel.Response>();
//                 //     if (string.Equals(getUserDetailResponseObject?.UserStatus, "UNCONFIRMED", StringComparison.OrdinalIgnoreCase))
//                 //     {
//                 //         if (!await HandleUnconfirmedUserAsync(mappedRequest?., "CustomerAPI"))
//                 //         {
//                 //             return (HttpStatusCode.BadRequest, resp);
//                 //         }
//                 //     }
//                 // }


//                 var signUpResponse = await _amazonCognitoMiddleware.SignUpAsync(new SignUpModel.Request
//                 {
//                     Username = dbResponse?.cognitoUserName.ToLower(),
//                     Password = Helper.GenerateRandomPassword(12),
//                     AttributeType = new List<SignUpModel.AttributeType>
//                     {
//                         new SignUpModel.AttributeType
//                         {
//                             Name = AttributeTypeName.PhoneNumber,
//                             Value = FormatPhoneNumber(dbResponse?.mobileNumber, countryCode)
//                         },
//                         new SignUpModel.AttributeType
//                         {
//                             Name = AttributeTypeName.Email,
//                             Value = dbResponse?.email
//                         }
//                     }
//                 });

//                 if (signUpResponse?.Code != ResponseCode.Success)
//                 {
//                     await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
//                         eventName: "registration_failed",
//                         stepCode: "identity",
//                         eventStatus: "failure",
//                         relatedEntityType: "registration",
//                         errorCode: ResponseCodes.Code400_BadRequest,
//                         errorMessage: signUpResponse?.Errors?.FirstOrDefault()?.ErrorMessage,
//                         properties: mappedRequest
//                     );

//                     _sqlTransactionHandler.RollbackTransaction();

//                     resp.code = ResponseCodes.Code400_BadRequest;
//                     resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                     resp.errors = new List<Errors>
//                     {
//                         new Errors
//                         {
//                             errorCode = ResponseCodes.Code400_BadRequest,
//                             errorMessage = signUpResponse?.Errors?.FirstOrDefault()?.ErrorMessage
//                         }
//                     };
//                     return (HttpStatusCode.BadRequest, resp);
//                 }

//                 await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
//                     eventName: "registration_started",
//                     stepCode: "identity",
//                     eventStatus: "success",
//                     relatedEntityType: "customer",
//                     relatedEntityId: dbResponse?.holdCustomerId,
//                     properties: mappedRequest
//                 );
//                 resp.code = ResponseCodes.Code200_Success;
//                 resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse.message);
//                 resp.data = new UserRegistrationResponseModel
//                 {
//                     holdCustomerId = _stringCipher.Encrypt(dbResponse.holdCustomerId),
//                     mobileNumber = dbResponse.mobileNumber,
//                     processId = dbResponse.processId,
//                     expiryTime = Convert.ToInt32(_configuration["UserRegistrationConfirmationCodeExpiryTimeInMilliseconds"])
//                 };
//                 _sqlTransactionHandler.CommitTransaction();
//                 return (HttpStatusCode.OK, resp);
//             }
//             catch (Exception ex)
//             {
//                 _sqlTransactionHandler.RollbackTransaction();
//                 throw new Exception("Error occurred.", ex);
//             }
//         }

//         private static string NormalizeDob(string dob)
//         {
//             if (string.IsNullOrWhiteSpace(dob) || dob == "--")
//                 return dob;

//             return string.Join("-", dob.Split('-')
//                 .Select(part =>
//                     int.TryParse(part, out var num) ? num.ToString("D2") : part
//                 ));
//         }

//         public async Task<(HttpStatusCode, object)> UserRegistrationValidationAsync(UserRegistrationValidationModel mappedRequest, ConversionSessionContext session)
//         {
//             var validationResult = await _validationHelper.ValidateModelAndSignatureAsync(mappedRequest, req => new UserRegistrationValidationModelValidator().Validate(req));
//             if (validationResult.Item1 != HttpStatusCode.OK)
//                 return validationResult;
//             var resp = new CommonApiResponse();

//             if (!string.IsNullOrEmpty(mappedRequest.holdCustomerId))
//                 mappedRequest.holdCustomerId = _stringCipher.Decrypt(mappedRequest.holdCustomerId);

//             var expiryTime = _configuration["UserRegistrationConfirmationCodeExpiryTimeInMilliseconds"];
//             var holdCustomerDetailResponse = await HoldCustomerDetail(mappedRequest.mobileNumber, mappedRequest.holdCustomerId, mappedRequest.processId, "Registration", expiryTime);

//             if (holdCustomerDetailResponse?.code != "0" || string.IsNullOrEmpty(holdCustomerDetailResponse?.cognitoUsername))
//             {
//                 resp.code = ResponseCodes.Code400_BadRequest;
//                 resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg400_BadRequest);
//                 resp.errors = new List<Errors>
//                 {
//                     new Errors
//                     {
//                         errorCode = holdCustomerDetailResponse?.message ,
//                         errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(holdCustomerDetailResponse?.message)
//                     }
//                 };
//                 return (HttpStatusCode.BadRequest, resp);
//             }

//             var verificationRequest = mappedRequest.MapObject<UserRegistrationVerificationRequestCommon>();

//             var getUserDetailResponse = await _amazonCognitoMiddleware.GetUserDetailAsync(new UserDetailModel.Request
//             {
//                 Username = holdCustomerDetailResponse.cognitoUsername.ToLower()
//             });

//             if (getUserDetailResponse?.Code == ResponseCode.Success)
//             {
//                 var getUserDetailResponseObject = getUserDetailResponse.Data.MapObject<UserDetailModel.Response>();
//                 verificationRequest.cognitoUserId = !string.IsNullOrEmpty(getUserDetailResponseObject?.Sub) ? getUserDetailResponseObject?.Sub : string.Empty;
//             }

//             _sqlTransactionHandler.BeginTransaction();
//             try
//             {
//                 var verificationResponse = await _userRegistrationRepository.UserRegistrationVerificationOTP(verificationRequest);
//                 if (verificationResponse.code != "0")
//                 {
//                     await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
//                         eventName: "otp_verification_failed",
//                         stepCode: "otp",
//                         eventStatus: "failure",
//                         relatedEntityType: "registration",
//                         errorCategory: "customer registration",
//                         errorCode: verificationResponse?.code ?? "",
//                         errorMessage: verificationResponse?.message ?? "",
//                         properties: mappedRequest
//                         );

//                     resp.code = ResponseCodes.Code400_BadRequest;
//                     resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg400_BadRequest);
//                     resp.errors = new List<Errors>
//                     {
//                         new Errors
//                         {
//                             errorCode = verificationResponse?.message,
//                             errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(verificationResponse?.message)
//                         }
//                     };
//                     _sqlTransactionHandler.RollbackTransaction();
//                     return (HttpStatusCode.BadRequest, resp);
//                 }

//                 var confirmSignUpResponse = await _amazonCognitoMiddleware.ConfirmSignUpAsync(new ConfirmSignUpModel.Request
//                 {
//                     Username = holdCustomerDetailResponse.cognitoUsername.ToLower(),
//                     ConfirmationCode = mappedRequest?.otpCode
//                 });

//                 if (confirmSignUpResponse?.Code != ResponseCode.Success)
//                 {
//                     await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
//                         eventName: "otp_verification_failed",
//                         stepCode: "otp",
//                         eventStatus: "failure",
//                         relatedEntityType: "registration",
//                         errorCategory: "customer registration",
//                         errorCode: verificationResponse?.code ?? "",
//                         errorMessage: verificationResponse?.message ?? "",
//                         properties: mappedRequest
//                     );

//                     resp.code = ResponseCodes.Code400_BadRequest;
//                     resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg400_BadRequest);
//                     resp.errors = new List<Errors>
//                     {
//                         new Errors
//                         {
//                             errorCode = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg400_BadRequest),
//                             errorMessage = confirmSignUpResponse?.Errors?.FirstOrDefault()?.ErrorMessage
//                         }
//                     };
//                     _sqlTransactionHandler.RollbackTransaction();
//                     return (HttpStatusCode.BadRequest, resp);
//                 }

//                 // if (!string.IsNullOrEmpty(verificationResponse?.email))
//                 // {
//                 //     var markEmailAsVerifiedResponse = await _amazonCognitoMiddleware.MarkEmailAsVerifiedAsync(new MarkEmailAsVerifiedModel.Request
//                 //     {
//                 //         Username = holdCustomerDetailResponse.cognitoUsername.ToLower(),
//                 //         Email = verificationResponse?.email
//                 //     });

//                 //     if (markEmailAsVerifiedResponse?.Code != ResponseCode.Success)
//                 //     {
//                 //         resp.code = ResponseCodes.Code400_BadRequest;
//                 //         resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                 //         resp.errors = new List<Errors>
//                 //     {
//                 //         new Errors
//                 //         {
//                 //             errorCode = ResponseCodes.Code400_BadRequest,
//                 //             errorMessage = markEmailAsVerifiedResponse.Errors?.FirstOrDefault()?.ErrorMessage
//                 //         }
//                 //     };
//                 //         _sqlTransactionHandler.RollbackTransaction();
//                 //         return (HttpStatusCode.BadRequest, resp);
//                 //     }
//                 // }

//                 await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
//                     eventName: "identity_resolved",
//                     stepCode: "identity",
//                     eventStatus: "success",
//                     relatedEntityType: "customer",
//                     relatedEntityId: verificationResponse.customerId,
//                     properties: mappedRequest
//                 );
//                 verificationResponse.customerId = _stringCipher.Encrypt(verificationResponse.customerId);
//                 resp.code = ResponseCodes.Code200_Success;
//                 resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(verificationResponse?.message);
//                 resp.data = JsonConvert.DeserializeObject<UserRegistrationVerificationResponseModel>(JsonConvert.SerializeObject(verificationResponse));
//                 _sqlTransactionHandler.CommitTransaction();
//                 return (HttpStatusCode.OK, resp);
//             }
//             catch (Exception ex)
//             {
//                 _sqlTransactionHandler.RollbackTransaction();
//                 throw new Exception("Error occurred.", ex);
//             }
//         }

//         public async Task<(HttpStatusCode, object)> CreateUserCredentialsAsync(CreatePasswordModel mappedRequest)
//         {
//             var validationResult = await _validationHelper.ValidateModelAndSignatureAsync(mappedRequest, req => new CreatePasswordModelValidator().Validate(req));
//             if (validationResult.Item1 != HttpStatusCode.OK)
//                 return validationResult;
//             var resp = new CommonApiResponse();
//             if (mappedRequest.password != mappedRequest.confirmPassword)
//             {
//                 resp.code = ResponseCodes.Code400_BadRequest;
//                 resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                 resp.errors = new List<Errors>
//                 {
//                     new Errors
//                     {
//                         errorCode = ResponseCodes.CE464_PasswordAndConfirmPasswordDoesnotMatch,
//                         errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CE464_PasswordAndConfirmPasswordDoesnotMatch)
//                     }
//                 };
//                 return (HttpStatusCode.BadRequest, resp);
//             }
//             if (!string.IsNullOrEmpty(mappedRequest.customerId))
//                 mappedRequest.customerId = _stringCipher.Decrypt(mappedRequest.customerId);

//             var customerDetailResponse = await _userBusiness.GetCustomerDetail(new shared.User.UserDetailRequestCommon
//             {
//                 customerId = mappedRequest?.customerId,
//                 mobileNumber = mappedRequest?.mobileNumber,
//                 processId = mappedRequest?.processId,
//                 processType = "Setpassword"
//             });

//             if (customerDetailResponse?.code != "0" || string.IsNullOrEmpty(customerDetailResponse?.cognitoUsername))
//             {
//                 resp.code = ResponseCodes.Code400_BadRequest;
//                 resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                 resp.errors = new List<Errors>
//                 {
//                     new Errors
//                     {
//                         errorCode = customerDetailResponse?.message,
//                         errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(customerDetailResponse?.message)
//                     }
//                 };
//                 return (HttpStatusCode.BadRequest, resp);
//             }

//             _sqlTransactionHandler.BeginTransaction();
//             try
//             {
//                 var response = await _userRegistrationRepository.UserCreatePasswordAsync(mappedRequest);

//                 if (response?.code != "0")
//                 {
//                     resp.code = ResponseCodes.Code400_BadRequest;
//                     resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                     resp.errors = new List<Errors>
//                     {
//                         new Errors
//                         {
//                             errorCode = response.message,
//                             errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(response.message)
//                         }
//                     };
//                     _sqlTransactionHandler.RollbackTransaction();
//                     return (HttpStatusCode.BadRequest, resp);
//                 }

//                 var setPasswordResponse = await _amazonCognitoMiddleware.SetPasswordAsync(new SetPasswordModel.Request
//                 {
//                     Username = customerDetailResponse.cognitoUsername.ToLower(),
//                     Password = mappedRequest.password,
//                     IsPermanent = true
//                 });

//                 if (setPasswordResponse?.Code != ResponseCode.Success)
//                 {
//                     resp.code = ResponseCodes.Code400_BadRequest;
//                     resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                     resp.errors = new List<Errors>
//                     {
//                         new Errors
//                         {
//                             errorCode =  ResponseCodes.Code400_BadRequest,
//                             errorMessage = setPasswordResponse?.Errors?.FirstOrDefault()?.ErrorMessage
//                         }
//                     };
//                     _sqlTransactionHandler.RollbackTransaction();
//                     return (HttpStatusCode.BadRequest, resp);
//                 }

//                 response.customerId = _stringCipher.Encrypt(response?.customerId);
//                 resp.code = ResponseCodes.Code200_Success;
//                 resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(response.message);
//                 resp.data = JsonConvert.DeserializeObject<CreatePasswordResponseModel>(JsonConvert.SerializeObject(response));
//                 _sqlTransactionHandler.CommitTransaction();
//                 return (HttpStatusCode.OK, resp);
//             }
//             catch (Exception ex)
//             {
//                 _sqlTransactionHandler.RollbackTransaction();
//                 throw new Exception("Error occurred.", ex);
//             }
//         }
//         public async Task<(HttpStatusCode, object)> UserResendOTPAsync(ResendOTPRequestModel mappedRequest)
//         {
//             var validationResult = await _validationHelper.ValidateModelAndSignatureAsync(mappedRequest, req => new ResendOTPRequestModelValidator().Validate(req));
//             if (validationResult.Item1 != HttpStatusCode.OK)
//                 return validationResult;

//             var resp = new CommonApiResponse();

//             if (!string.IsNullOrEmpty(mappedRequest.holdCustomerId))
//                 mappedRequest.holdCustomerId = _stringCipher.Decrypt(mappedRequest.holdCustomerId);

//             _sqlTransactionHandler.BeginTransaction();
//             try
//             {
//                 var userResendOTPResponse = await _userRegistrationRepository.UserResendOTPAsync(mappedRequest);

//                 if (userResendOTPResponse.code != "0")
//                 {
//                     resp.code = ResponseCodes.Code400_BadRequest;
//                     resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg400_BadRequest);
//                     resp.errors = new List<Errors>
//                     {
//                         new Errors
//                         {
//                             errorCode = userResendOTPResponse.message,
//                             errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(userResendOTPResponse.message)
//                         }
//                     };
//                     _sqlTransactionHandler.RollbackTransaction();
//                     return (HttpStatusCode.BadRequest, resp);
//                 }
//                 var resendSignUpConfirmationCodeResponse = await _amazonCognitoMiddleware.ResendSignUpConfirmationCodeAsync(new ResendSignUpConfirmationCodeModel.Request
//                 {
//                     Username = userResendOTPResponse?.cognitoUsername.ToLower()
//                 });

//                 if (resendSignUpConfirmationCodeResponse.Code != ResponseCode.Success)
//                 {
//                     resp.code = ResponseCodes.Code400_BadRequest;
//                     resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                     resp.errors = new List<Errors>
//                     {
//                         new Errors
//                         {
//                             errorCode = ResponseCodes.Code400_BadRequest,
//                             errorMessage = resendSignUpConfirmationCodeResponse.Errors?.FirstOrDefault()?.ErrorMessage
//                         }
//                     };
//                     _sqlTransactionHandler.RollbackTransaction();
//                     return (HttpStatusCode.BadRequest, resp);
//                 }
//                 resp.code = ResponseCodes.Code200_Success;
//                 resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(userResendOTPResponse?.message);
//                 resp.data = new ResendOTPResponseModel
//                 {
//                     holdCustomerId = _stringCipher.Encrypt(userResendOTPResponse?.holdCustomerId),
//                     mobileNumber = userResendOTPResponse.mobileNumber,
//                     processId = userResendOTPResponse.processId,
//                     expiryTime = Convert.ToInt32(_configuration["UserRegistrationConfirmationCodeExpiryTimeInMilliseconds"])
//                 };
//                 _sqlTransactionHandler.CommitTransaction();
//                 return (HttpStatusCode.OK, resp);
//             }
//             catch (Exception ex)
//             {
//                 _sqlTransactionHandler.RollbackTransaction();
//                 throw new Exception("Error occurred.", ex);
//             }
//         }
//         public async Task<(HttpStatusCode, object)> ForgotUserPasswordAsync(ForgotPasswordRequestModel mappedRequest)
//         {
//             var validationResult = await _validationHelper.ValidateModelAndSignatureAsync(mappedRequest, req => new ForgotPasswordRequestModelValidator().Validate(req));
//             if (validationResult.Item1 != HttpStatusCode.OK)
//                 return validationResult;
//             var resp = new CommonApiResponse();
//             _sqlTransactionHandler.BeginTransaction();
//             try
//             {
//                 var response = await _userRegistrationRepository.ForgotUserPasswordAsync(mappedRequest);
//                 if (response?.code != "0" || string.IsNullOrEmpty(response?.cognitoUsername))
//                 {
//                     resp.code = ResponseCodes.Code400_BadRequest;
//                     resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                     resp.errors = new List<Errors>
//                     {
//                         new Errors
//                         {
//                             errorCode = response.message,
//                             errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(response.message)
//                         }
//                     };
//                     _sqlTransactionHandler.RollbackTransaction();
//                     return (HttpStatusCode.BadRequest, resp);
//                 }

//                 var forgotPasswordResponse = await _amazonCognitoMiddleware.ForgotPasswordAsync(new ForgotPasswordModel.Request
//                 {
//                     Username = response?.cognitoUsername.ToLower()
//                 });

//                 if (forgotPasswordResponse.Code != ResponseCode.Success)
//                 {
//                     resp.code = ResponseCodes.Code400_BadRequest;
//                     resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                     resp.errors = new List<Errors>
//                     {
//                         new Errors
//                         {
//                             errorCode = ResponseCodes.Code400_BadRequest,
//                             errorMessage = forgotPasswordResponse.Errors?.FirstOrDefault()?.ErrorMessage
//                         }
//                     };
//                     _sqlTransactionHandler.RollbackTransaction();
//                     return (HttpStatusCode.BadRequest, resp);
//                 }

//                 response.customerId = _stringCipher.Encrypt(response.customerId);
//                 resp.code = ResponseCodes.Code200_Success;
//                 resp.message = response.message ?? ResponseMessages.Msg200_Success;
//                 resp.data = new ForgotPasswordResponseModel
//                 {
//                     customerId = response.customerId,
//                     mobileNumber = response.MobileNumber,
//                     processId = response.processId,
//                     expiryTime = Convert.ToInt32(_configuration["ForgotPasswordConfirmationCodeExpiryTimeInMilliseconds"])
//                 };
//                 _sqlTransactionHandler.CommitTransaction();
//                 return (HttpStatusCode.OK, resp);
//             }
//             catch (Exception ex)
//             {
//                 _sqlTransactionHandler.RollbackTransaction();
//                 throw new Exception("Error occurred.", ex);
//             }
//         }
//         public async Task<(HttpStatusCode, object)> ForgotPasswordOTPVerificationAsync(ForgotPasswordOTPVerificatonRequestModel mappedRequest)
//         {
//             var validationResult = await _validationHelper.ValidateModelAndSignatureAsync(mappedRequest, req => new ForgotPasswordOTPVerificatonRequestModelValidator().Validate(req));
//             if (validationResult.Item1 != HttpStatusCode.OK)
//                 return validationResult;
//             var resp = new CommonApiResponse();

//             if (!string.IsNullOrEmpty(mappedRequest.customerId))
//                 mappedRequest.customerId = _stringCipher.Decrypt(mappedRequest.customerId);

//             var expiryTime = _configuration["UserRegistrationConfirmationCodeExpiryTimeInMilliseconds"];
//             var holdCustomerDetailResponse = await HoldCustomerDetail(mappedRequest.mobileNumber, mappedRequest.customerId, mappedRequest.processId, "ForgotPassword", expiryTime);

//             if (holdCustomerDetailResponse?.code != "0" || string.IsNullOrEmpty(holdCustomerDetailResponse?.cognitoUsername))
//             {
//                 resp.code = ResponseCodes.Code400_BadRequest;
//                 resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                 resp.errors = new List<Errors>
//                 {
//                     new Errors
//                     {
//                         errorCode = holdCustomerDetailResponse?.message,
//                         errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(holdCustomerDetailResponse?.message)
//                     }
//                 };
//                 return (HttpStatusCode.BadRequest, resp);
//             }

//             _sqlTransactionHandler.BeginTransaction();
//             try
//             {
//                 var response = await _userRegistrationRepository.ForgotPasswordOTPVerificationAsync(mappedRequest);
//                 if (response?.code != "0" || string.IsNullOrEmpty(response?.cognitoUsername))
//                 {
//                     resp.code = ResponseCodes.Code400_BadRequest;
//                     resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg400_BadRequest);
//                     resp.errors = new List<Errors>
//                     {
//                         new Errors
//                         {
//                             errorCode = response.message,
//                             errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(response.message)
//                         }
//                     };
//                     _sqlTransactionHandler.RollbackTransaction();
//                     return (HttpStatusCode.BadRequest, resp);
//                 }

//                 var confirmForgotPasswordResponse = await _amazonCognitoMiddleware.ConfirmForgotPasswordAsync(new ConfirmForgotPasswordModel.Request
//                 {
//                     Username = response?.cognitoUsername.ToLower(),
//                     ConfirmationCode = mappedRequest?.otpCode,
//                     NewPassword = Helper.GenerateRandomPassword(12)
//                 });

//                 if (confirmForgotPasswordResponse?.Code != ResponseCode.Success)
//                 {
//                     resp.code = ResponseCodes.Code400_BadRequest;
//                     resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                     resp.errors = new List<Errors>
//                     {
//                         new Errors
//                         {
//                             errorCode = ResponseCodes.Code400_BadRequest,
//                             errorMessage = confirmForgotPasswordResponse?.Errors?.FirstOrDefault()?.ErrorMessage
//                         }
//                     };
//                     _sqlTransactionHandler.RollbackTransaction();
//                     return (HttpStatusCode.BadRequest, resp);
//                 }

//                 response.customerId = _stringCipher.Encrypt(response?.customerId);
//                 resp.code = ResponseCodes.Code200_Success;
//                 resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(response.message);
//                 resp.data = JsonConvert.DeserializeObject<ForgotPasswordVerificationResponse>(JsonConvert.SerializeObject(response));
//                 _sqlTransactionHandler.CommitTransaction();
//                 return (HttpStatusCode.OK, resp);
//             }
//             catch (Exception ex)
//             {
//                 _sqlTransactionHandler.RollbackTransaction();
//                 throw new Exception("Error occurred.", ex);
//             }
//         }
//         public async Task<(HttpStatusCode, object)> ForgotPasswordResendOTPAsync(ForgotPasswordResendOTPModel mappedRequest)
//         {
//             var validationResult = await _validationHelper.ValidateModelAndSignatureAsync(mappedRequest, req => new ForgotPasswordResendOTPModelValidator().Validate(req));
//             if (validationResult.Item1 != HttpStatusCode.OK)
//                 return validationResult;

//             var resp = new CommonApiResponse();

//             if (!string.IsNullOrEmpty(mappedRequest.customerId))
//                 mappedRequest.customerId = _stringCipher.Decrypt(mappedRequest.customerId);

//             _sqlTransactionHandler.BeginTransaction();
//             try
//             {
//                 var response = await _userRegistrationRepository.ForgotPasswordResendOTPAsync(mappedRequest);
//                 if (response?.code != "0" || string.IsNullOrEmpty(response?.cognitoUsername))
//                 {
//                     resp.code = ResponseCodes.Code400_BadRequest;
//                     resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                     resp.errors = new List<Errors>
//                     {
//                         new Errors
//                         {
//                             errorCode = response.message,
//                             errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(response.message)
//                         }
//                     };
//                     _sqlTransactionHandler.RollbackTransaction();
//                     return (HttpStatusCode.BadRequest, resp);
//                 }

//                 var forgotPasswordResponse = await _amazonCognitoMiddleware.ForgotPasswordAsync(new ForgotPasswordModel.Request
//                 {
//                     Username = response?.cognitoUsername.ToLower()
//                 });

//                 if (forgotPasswordResponse.Code != ResponseCode.Success)
//                 {
//                     resp.code = ResponseCodes.Code400_BadRequest;
//                     resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                     resp.errors = new List<Errors>
//                     {
//                         new Errors
//                         {
//                             errorCode = ResponseCodes.Code400_BadRequest,
//                             errorMessage = forgotPasswordResponse.Errors?.FirstOrDefault()?.ErrorMessage
//                         }
//                     };
//                     _sqlTransactionHandler.RollbackTransaction();
//                     return (HttpStatusCode.BadRequest, resp);
//                 }

//                 response.customerId = _stringCipher.Encrypt(response.customerId);
//                 resp.code = ResponseCodes.Code200_Success;
//                 resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(response.message);
//                 resp.data = new ForgotPasswordResendResponseModel
//                 {
//                     customerId = response.customerId,
//                     mobileNumber = response.mobileNumber,
//                     processId = response.processId,
//                     expiryTime = Convert.ToInt32(_configuration["ForgotPasswordConfirmationCodeExpiryTimeInMilliseconds"])
//                 };
//                 _sqlTransactionHandler.CommitTransaction();
//                 return (HttpStatusCode.OK, resp);
//             }
//             catch (Exception ex)
//             {
//                 _sqlTransactionHandler.RollbackTransaction();
//                 throw new Exception("Error occurred.", ex);
//             }
//         }
//         public async Task<(HttpStatusCode, object)> ChangeCustomerPasswordAsync(ChangeCustomerPasswordRequestModel mappedRequest)
//         {
//             var validationResult = await _validationHelper.ValidateModelAndSignatureAsync(mappedRequest, req => new ChangeCustomerPasswordRequestModelValidator().Validate(req));
//             if (validationResult.Item1 != HttpStatusCode.OK)
//                 return validationResult;

//             var resp = new CommonApiResponse();
//             var request = mappedRequest.MapObject<CreatePasswordRequestCommon>();
//             var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(claims => claims.Type == UserClaimsName.UserId)?.Value;
//             var agentId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(claims => claims.Type == UserClaimsName.AgentId)?.Value;
//             if (request.newPassword != request.confirmPassword)
//             {
//                 resp.code = ResponseCodes.Code400_BadRequest;
//                 resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                 resp.errors = new List<Errors>
//                 {
//                     new Errors
//                     {
//                         errorCode = ResponseCodes.CUEREG109_NewPasswordAndConfirmPasswordNotMatched,
//                         errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUEREG109_NewPasswordAndConfirmPasswordNotMatched)
//                     }
//                 };
//                 return (HttpStatusCode.BadRequest, resp);
//             }
//             userId = _stringCipher.Decrypt(userId) ?? string.Empty;
//             agentId = _stringCipher.Decrypt(agentId) ?? string.Empty;
//             request.actionUserId = userId;

//             var customerDetailResponse = await _userBusiness.GetCustomerDetail(new shared.User.UserDetailRequestCommon
//             {
//                 customerId = agentId,
//             });

//             if (customerDetailResponse?.code != "0" || string.IsNullOrEmpty(customerDetailResponse?.cognitoUsername))
//             {
//                 resp.message = ResponseMessages.Msg400_BadRequest;
//                 resp.errors = new List<Errors>
//                 {
//                     new Errors{errorCode=customerDetailResponse?.message,errorMessage=await _apiResponseMessageService.GetMessageByCodeAsync(customerDetailResponse?.message)   }
//                 };
//                 return (HttpStatusCode.BadRequest, resp);
//             }

//             if (!hoslog.customer.common.Utilities.SqlServerCrypto.PWDCOMPARE(request?.oldPassword, customerDetailResponse.Password))
//             {
//                 resp.code = ResponseCodes.Code400_BadRequest;
//                 resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                 resp.errors = new List<Errors>
//                 {
//                     new Errors
//                     {
//                         errorCode = ResponseCodes.CUEREG110_InvalidCurrentPassword,
//                         errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUEREG110_InvalidCurrentPassword)
//                     }
//                 };
//                 return (HttpStatusCode.BadRequest, resp);
//             }

//             if (hoslog.customer.common.Utilities.SqlServerCrypto.PWDCOMPARE(request?.newPassword, customerDetailResponse.Password))
//             {
//                 resp.code = ResponseCodes.Code400_BadRequest;
//                 resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                 resp.errors = new List<Errors>
//                 {
//                     new Errors
//                     {
//                         errorCode = ResponseCodes.CUEREG111_NewPasswordCannotBeSameAsCurrentPassword,
//                         errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUEREG111_NewPasswordCannotBeSameAsCurrentPassword)
//                     }
//                 };
//                 return (HttpStatusCode.BadRequest, resp);
//             }

//             string authorizationHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString();
//             var token = authorizationHeader.StartsWith("Bearer ")
//                    ? authorizationHeader.Substring("Bearer ".Length).Trim()
//                    : null;
//             if (string.IsNullOrEmpty(token))
//             {
//                 resp.code = ResponseCodes.Code400_BadRequest;
//                 resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                 return (HttpStatusCode.BadRequest, resp);
//             }

//             _sqlTransactionHandler.BeginTransaction();
//             try
//             {
//                 var response = await _userRegistrationRepository.ChangeCustomerPasswordAsync(request);
//                 if (response?.code != "0")
//                 {
//                     resp.code = ResponseCodes.Code400_BadRequest;
//                     resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                     resp.errors = new List<Errors>
//                     {
//                         new Errors
//                         {
//                             errorCode = response?.message,
//                             errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(response?.message)
//                         }
//                     };
//                     _sqlTransactionHandler.RollbackTransaction();
//                     return (HttpStatusCode.BadRequest, resp);
//                 }

//                 var changePasswordResponse = await _amazonCognitoMiddleware.ChangePasswordAsync(new ChangePasswordModel.Request
//                 {
//                     AccessToken = token,
//                     OldPassword = request.oldPassword,
//                     NewPassword = request.newPassword
//                 });

//                 if (changePasswordResponse?.Code != ResponseCode.Success)
//                 {
//                     resp.code = ResponseCodes.Code400_BadRequest;
//                     resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                     resp.errors = new List<Errors>
//                     {
//                         new Errors
//                         {
//                             errorCode = ResponseCodes.Code400_BadRequest,
//                             errorMessage = changePasswordResponse?.Errors?.FirstOrDefault()?.ErrorMessage
//                         }
//                     };
//                     _sqlTransactionHandler.RollbackTransaction();
//                     return (HttpStatusCode.BadRequest, resp);
//                 }

//                 await _amazonCognitoMiddleware.SignOutAsync(new SignOutModel.Request
//                 {
//                     AccessToken = token
//                 });

//                 response.customerId = _stringCipher.Encrypt(response.customerId);
//                 resp.code = ResponseCodes.Code200_Success;
//                 resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(response.message);
//                 resp.data = JsonConvert.DeserializeObject<ChangeCustomerPasswordResponseModel>(JsonConvert.SerializeObject(response));
//                 _sqlTransactionHandler.CommitTransaction();
//                 return (HttpStatusCode.OK, resp);
//             }
//             catch (Exception ex)
//             {
//                 _sqlTransactionHandler.RollbackTransaction();
//                 throw new Exception("Error occurred.", ex);
//             }
//         }

//         public async Task<(HttpStatusCode, object)> ForgotPasswordSetOTPAsync(ForgotSetPasswordModel mappedRequest)
//         {
//             var validationResult = await _validationHelper.ValidateModelAndSignatureAsync(mappedRequest, req => new ForgotSetPasswordModelValidator().Validate(req));
//             if (validationResult.Item1 != HttpStatusCode.OK)
//                 return validationResult;

//             var resp = new CommonApiResponse();
//             var request = mappedRequest.MapObject<ForgotSetPasswordCommon>();
//             if (string.IsNullOrWhiteSpace(mappedRequest.customerId))
//             {
//                 resp.code = ResponseCodes.Code400_BadRequest;
//                 resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                 resp.errors = new List<Errors>
//                 {
//                     new Errors
//                     {
//                         errorCode = ResponseCodes.CE465_CustomerIdRequired,
//                         errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CE465_CustomerIdRequired)
//                     }
//                 };
//                 return (HttpStatusCode.BadRequest, resp);
//             }
//             if (request.password != request.confirmPassword)
//             {
//                 resp.code = ResponseCodes.Code400_BadRequest;
//                 resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                 resp.errors = new List<Errors>
//                 {
//                     new Errors
//                     {
//                         errorCode = ResponseCodes.CUEREG109_NewPasswordAndConfirmPasswordNotMatched,
//                         errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUEREG109_NewPasswordAndConfirmPasswordNotMatched)
//                     }
//                 };
//                 return (HttpStatusCode.BadRequest, resp);
//             }

//             request.customerId = _stringCipher.Decrypt(mappedRequest.customerId);
//             var customerDetailResponse = await _userBusiness.GetCustomerDetail(new shared.User.UserDetailRequestCommon
//             {
//                 customerId = request?.customerId,
//                 processId = request?.processId,
//                 processType = "Setpassword"
//             });

//             if (customerDetailResponse?.code != "0" || string.IsNullOrEmpty(customerDetailResponse?.cognitoUsername))
//             {
//                 resp.code = ResponseCodes.Code400_BadRequest;
//                 resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                 resp.errors = new List<Errors>
//                 {
//                     new Errors
//                     {
//                         errorCode = customerDetailResponse?.message,
//                         errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(customerDetailResponse?.message)
//                     }
//                 };
//                 return (HttpStatusCode.BadRequest, resp);
//             }

//             if (hoslog.customer.common.Utilities.SqlServerCrypto.PWDCOMPARE(request?.password, customerDetailResponse.Password))
//             {
//                 resp.code = ResponseCodes.Code400_BadRequest;
//                 resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                 resp.errors = new List<Errors>
//                 {
//                     new Errors
//                     {
//                         errorCode = ResponseCodes.CUEREG111_NewPasswordCannotBeSameAsCurrentPassword,
//                         errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUEREG111_NewPasswordCannotBeSameAsCurrentPassword)
//                     }
//                 };
//                 return (HttpStatusCode.BadRequest, resp);
//             }

//             _sqlTransactionHandler.BeginTransaction();
//             try
//             {
//                 var response = await _userRegistrationRepository.ForgotPasswordSetOTPAsync(request);
//                 if (response.code != "0")
//                 {
//                     resp.code = ResponseCodes.Code400_BadRequest;
//                     resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                     resp.errors = new List<Errors>
//                     {
//                         new Errors
//                         {
//                             errorCode = response.message,
//                             errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(response.message)
//                         }
//                     };
//                     _sqlTransactionHandler.RollbackTransaction();
//                     return (HttpStatusCode.BadRequest, resp);
//                 }

//                 var setPasswordResponse = await _amazonCognitoMiddleware.SetPasswordAsync(new SetPasswordModel.Request
//                 {
//                     Username = response?.cognitoUsername.ToLower(),
//                     Password = request.password,
//                     IsPermanent = true
//                 });

//                 if (setPasswordResponse?.Code != ResponseCode.Success)
//                 {
//                     resp.code = ResponseCodes.Code400_BadRequest;
//                     resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
//                     resp.errors = new List<Errors>
//                     {
//                         new Errors
//                         {
//                             errorCode = ResponseCodes.Code400_BadRequest,
//                             errorMessage = setPasswordResponse?.Errors?.FirstOrDefault()?.ErrorMessage
//                         }
//                     };
//                     _sqlTransactionHandler.RollbackTransaction();
//                     return (HttpStatusCode.BadRequest, resp);
//                 }

//                 response.customerId = _stringCipher.Encrypt(response.customerId);
//                 resp.code = ResponseCodes.Code200_Success;
//                 resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(response.message);
//                 resp.data = JsonConvert.DeserializeObject<ForgotSetPasswordResponse>(JsonConvert.SerializeObject(response));
//                 _sqlTransactionHandler.CommitTransaction();
//                 return (HttpStatusCode.OK, resp);
//             }
//             catch (Exception ex)
//             {
//                 _sqlTransactionHandler.RollbackTransaction();
//                 throw new Exception("Error occurred while updating user details.", ex);
//             }
//         }

//         public async Task<HoldCustomerDetailCommon> HoldCustomerDetail(string mobileNumber, string holdCustomerId, string processId = "", string processType = "", string expiryTime = "")
//         {
//             return await _userRegistrationRepository.HoldCustomerDetail(mobileNumber, holdCustomerId, processId, processType, expiryTime);
//         }

//         private string FormatPhoneNumber(string phoneNumber, string countryCode = "+81")
//         {
//             if (phoneNumber.StartsWith("0"))
//                 phoneNumber = phoneNumber.Substring(1);
//             return $"{countryCode}{phoneNumber}";
//         }
//         private async Task TrackConversionEvent(
//            ConversionSessionContext session,
//            Guid? journeyGuid,
//            string eventName,
//            string stepCode,
//            string eventStatus,
//            string relatedEntityType,
//            string errorCategory = "",
//            string errorCode = "",
//            string errorMessage = "",
//            string relatedEntityId = null,
//            object properties = null)
//         {
//             try
//             {
//                 if (await ShouldBlockConversionFlowAsync(session))
//                 {
//                     _logger.LogInformation($"Conversion flow blocked: Journey is cancelled. Skipping event {eventName}");
//                     return;
//                 }
//                 if (journeyGuid == null || journeyGuid == Guid.Empty)
//                     return;

//                 await _conversion.InsertEventAsync(new ConversionEventRequestCommon()
//                 {
//                     eventId = Guid.NewGuid(),
//                     reservationJourneyId = journeyGuid,
//                     conversionSessionSno = _stringCipher.Decrypt(session.conversionSessionSno),
//                     eventName = eventName,
//                     stepCode = stepCode,
//                     eventStatus = eventStatus,
//                     eventSource = "backend",
//                     actorCustomerId = _stringCipher.Decrypt(session.currentCustomerId),
//                     authenticationState = _stringCipher.Decrypt(session.currentCustomerId) != "0" ? "authenticated" : "guest",
//                     relatedEntityType = relatedEntityType,
//                     relatedEntityId = relatedEntityId,
//                     pageUrl = _httpContextAccessor?.HttpContext?.Request.Headers.Origin.ToString(),
//                     timezoneOffsetMinutes = _httpContextAccessor?.HttpContext?.Items["TimezoneOffsetMinutes"]?.ToString(),
//                     previousPageUrl = _httpContextAccessor?.HttpContext?.Request.Headers.Referer.ToString(),
//                     clientUTCDate = DateTime.UtcNow,
//                     errorCategory = errorCategory,
//                     errorCode = errorCode,
//                     errorMessageSafe = errorMessage,
//                     propertiesJson = properties != null ? System.Text.Json.JsonSerializer.Serialize(properties) : null,
//                     pageType = $"reservation_{stepCode}",
//                     requestTraceId = Activity.Current?.TraceId.ToString(),
//                 });
//             }
//             catch (Exception ex)
//             {
//                 // Log but don't throw - event tracking should not break the main flow
//                 Console.WriteLine($"Failed to track conversion event {eventName}: {ex.Message}");
//             }
//         }
//         private Guid? GetJourneyGuid()
//         {
//             try
//             {
//                 var headerJourneyId = _httpContextAccessor.HttpContext?.Request.Headers["X-Reservation-Journey-Id"].FirstOrDefault();
//                 if (!string.IsNullOrEmpty(headerJourneyId) && Guid.TryParse(headerJourneyId, out var journeyGuid))
//                 {
//                     return journeyGuid;
//                 }

//                 return null;
//             }
//             catch
//             {
//                 return null;
//             }
//         }
//         private async Task<bool> ShouldBlockConversionFlowAsync(ConversionSessionContext session)
//         {
//             try
//             {
//                 var journeyGuid = GetJourneyGuid();
//                 if (journeyGuid == null || journeyGuid == Guid.Empty)
//                 {
//                     return false;
//                 }

//                 var journey = await _conversion.GetJourneyAsync(journeyGuid.Value);

//                 return !ConversionJourneyService.EnsureJourneyCanContinue(
//                     _httpContextAccessor.HttpContext!.Request,
//                     journey,
//                     long.Parse(_stringCipher.Decrypt(session.currentCustomerId)),
//                     _logger);
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Failed to check journey status for conversion blocking");
//                 return false;
//             }
//         }
//     }
// }
