using System.Diagnostics;
using hoslog.customer.api.Models.UserRegistration;
using hoslog.customer.business.User;
using hoslog.customer.common;
using hoslog.customer.common.Middleware.AmazonCognito;
using hoslog.customer.common.Models.Auth;
using hoslog.customer.common.Models.Middleware.AmazonCognito;
using hoslog.customer.common.Models.Middleware.AmazonCognitoModel.Auth;
using hoslog.customer.common.Models.Middleware.AmazonCognitoModel.Password;
using hoslog.customer.common.Models.Middleware.AmazonCognitoModel.SignUp;
using hoslog.customer.common.Models.Middleware.AmazonCognitoModel.UserManagement;
using hoslog.customer.common.Service;
using hoslog.customer.common.Utilities;
using hoslog.customer.domain.UserRegistration;
using hoslog.customer.shared;
using hoslog.customer.shared.UserRegistration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Text.RegularExpressions;
using hoslog.customer.business.Conversion;
using hoslog.customer.common.Models.Conversion;
using hoslog.customer.domain.Conversion;
using hoslog.customer.shared.Conversion;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using hoslog.customer.business.ExceptionLog;
using hoslog.customer.domain.Profile;
using hoslog.customer.domain.Reservation;
using hoslog.customer.shared.ReservationModel;

namespace hoslog.customer.business.UserRegistration
{
    public class UserRegistrationBusiness : IUserRegistrationBusiness
    {
        private readonly ILogger<UserRegistrationBusiness> _logger;
        private readonly IConversionRepository _conversion;
        private readonly IUserRegistrationRepository _userRegistrationRepository;
        private readonly CommonResponseHandler _commonResponseHandler;
        private readonly ValidationHelper _validationHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly StringCipher _stringCipher;
        private readonly AmazonCognitoMiddleware _amazonCognitoMiddleware;
        private readonly IConfiguration _configuration;
        private readonly IUserBusiness _userBusiness;
        private readonly SqlTransactionHandler _sqlTransactionHandler;
        private readonly ApiResponseMessageService _apiResponseMessageService;
        private readonly IExceptionLogBusiness _exceptionLogBusiness;
        private readonly IProfileRepository _profileRepository;
        private readonly IReservationRepository _reservationRepository; //same so used reservation repository

        public UserRegistrationBusiness(IConversionRepository conversion, ILogger<UserRegistrationBusiness> logger, IUserRegistrationRepository userRegistrationRepository, ValidationHelper validationHelper,
            IHttpContextAccessor httpContextAccessor, StringCipher stringCipher, AmazonCognitoMiddleware amazonCognitoMiddleware,
            IConfiguration configuration, IUserBusiness userBusiness, SqlTransactionHandler sqlTransactionHandler, CommonResponseHandler commonResponseHandler,
            ApiResponseMessageService apiResponseMessageService, IExceptionLogBusiness exceptionLogBusiness,
            IProfileRepository profileRepository, IReservationRepository reservationRepository
        )
        {
            _conversion = conversion;
            _logger = logger;
            _userRegistrationRepository = userRegistrationRepository;
            _commonResponseHandler = commonResponseHandler;
            _validationHelper = validationHelper;
            _httpContextAccessor = httpContextAccessor;
            _stringCipher = stringCipher;
            _amazonCognitoMiddleware = amazonCognitoMiddleware;
            _configuration = configuration;
            _userBusiness = userBusiness;
            _sqlTransactionHandler = sqlTransactionHandler;
            _apiResponseMessageService = apiResponseMessageService;
            _exceptionLogBusiness = exceptionLogBusiness;
            _profileRepository = profileRepository;
            _reservationRepository = reservationRepository;
        }

        public async Task<(HttpStatusCode, object)> UserRegistrationAsync(UserRegistrationModel mappedRequest, ConversionSessionContext session)
        {
            var validationResult = await _validationHelper.ValidateModelAndSignatureAsync(mappedRequest, req => new UserRegistrationModelValidator().Validate(req));
            if (validationResult.Item1 != HttpStatusCode.OK)
                return validationResult;
            var resp = new CommonApiResponse();
            if (mappedRequest.isCheckedTermsAndCondition != true)
            {
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                resp.data = Array.Empty<string>();
                resp.errors = new List<Errors>
                {
                    new Errors
                    {
                        errorCode = ResponseCodes.CUEREG100_RegistrationMustAgreeTermsAndCondition,
                        errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUEREG100_RegistrationMustAgreeTermsAndCondition)
                    }
                };
                return (HttpStatusCode.BadRequest, resp);
            }

            if (mappedRequest.isCheckedTermsAndCondition == false)
                return await _commonResponseHandler.HandleBadRequestResponse<CommonApiResponse>();

            var pattern = @"^(?:\d{4}[-/]{2}|\d{4}[-/]{2}\d{1,2}|\d{4}[-/]\d{1,2}[-/]{2}|[-/]{2}\d{1,2}|-?\d{1,2}[-/]\d{1,2}|\d{4}[-/]\d{1,2}[-/]\d{1,2}|-?\d{1,2}[-/]|[-/]{2})$";

            if (!string.IsNullOrWhiteSpace(mappedRequest?.dob) &&
                mappedRequest.dob.Trim() != "--" &&
                !Regex.IsMatch(mappedRequest.dob.Trim(), pattern))
            {
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUE120_InvalidDateFormat.ToString());
                resp.data = Array.Empty<string>();
                resp.errors = new List<Errors>
                        {
                            new Errors { errorCode = ResponseCodes.CUE120_InvalidDateFormat,errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUE120_InvalidDateFormat.ToString()) }
                        };
                return (HttpStatusCode.BadRequest, resp);
            }
            mappedRequest.dob = NormalizeDob(mappedRequest?.dob);

            var commonDBRequest = mappedRequest.MapObject<UserRegistrationCommon>();
            commonDBRequest.actionIP = _httpContextAccessor?.HttpContext?.GetIpAddress();
            commonDBRequest.actionUserId = mappedRequest?.mobileNumber;

            _sqlTransactionHandler.BeginTransaction();
            try
            {
                var dbResponse = await _userRegistrationRepository.UserRegistrationAsync(commonDBRequest);

                if (dbResponse.code != "0")
                {
                    await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
                        eventName: "registration_failed",
                        stepCode: "registration",
                        eventStatus: "identity",
                        relatedEntityType: "registration",
                        errorCode: dbResponse?.code ?? "",
                        errorMessage: dbResponse?.message ?? "",
                        properties: mappedRequest
                      );

                    resp.code = ResponseCodes.Code400_BadRequest;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                    resp.errors = new List<Errors>
                    {
                        new Errors
                        {
                            errorCode = dbResponse.message,
                            errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse.message)
                        }
                    };
                    _sqlTransactionHandler.RollbackTransaction();
                    return (HttpStatusCode.BadRequest, resp);
                }

                var countryCode = _configuration["CountryCode"];

                var formattedMobileNumber = FormatPhoneNumber(mappedRequest?.mobileNumber, countryCode);
                var checkUserExistsResponse = await _amazonCognitoMiddleware.CheckUserExistsAsync(formattedMobileNumber);
                var checkUserExistsResponseObject = checkUserExistsResponse?.MapObject<common.Models.Middleware.AmazonCognitoModel.UserManagement.CheckAttributeExistsWithDetailsModel.Response>()
                                                    ?? new common.Models.Middleware.AmazonCognitoModel.UserManagement.CheckAttributeExistsWithDetailsModel.Response();

                async Task<bool> HandleUnconfirmedUserAsync(string username, string actionPlatform)
                {
                    var adminDeleteResponse = await _amazonCognitoMiddleware.AdminDeleteAccountAsync(new common.Models.Middleware.AmazonCognitoModel.UserManagement.AdminDeleteAccountModel.Request
                    {
                        userName = username
                    });

                    if (adminDeleteResponse == null || adminDeleteResponse?.Code != hoslog.customer.common.Models.Middleware.AmazonCognito.ResponseCode.Success)
                    {
                        resp.code = ResponseCodes.Code400_BadRequest;
                        resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                        resp.errors = new List<Errors>
                        {
                            new Errors
                            {
                                errorCode = ResponseCodes.CUEREG101_DuplicateMobileNumberOrNickname,
                                errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUEREG101_DuplicateMobileNumberOrNickname)
                            }
                        };
                        _sqlTransactionHandler.RollbackTransaction();
                        return false;
                    }

                    var removeUnconfirmedCustomerInformationResponse = await _userRegistrationRepository.RemoveUnconfirmedCustomerInformation(new RemoveUnconfirmedCustomerInformationCommon
                    {
                        nickName = username,
                        actionIP = _httpContextAccessor?.HttpContext?.GetIpAddress(),
                        actionPlatform = actionPlatform
                    });

                    if (removeUnconfirmedCustomerInformationResponse.Code != ResponseCode.Success)
                    {
                        resp.code = ResponseCodes.Code400_BadRequest;
                        resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                        resp.errors = new List<Errors>
                        {
                            new Errors
                            {
                                errorCode = ResponseCodes.CUEREG101_DuplicateMobileNumberOrNickname,
                                errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUEREG101_DuplicateMobileNumberOrNickname)
                            }
                        };
                        _sqlTransactionHandler.RollbackTransaction();
                        return false;
                    }

                    return true;
                }

                // Handle each user in checkUserExistsResponseObject
                foreach (var user in checkUserExistsResponseObject?.Users)
                {
                    if (string.Equals(user?.UserStatus, "UNCONFIRMED", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!await HandleUnconfirmedUserAsync(user.Username, "CustomerAPI"))
                        {
                            return (HttpStatusCode.BadRequest, resp);
                        }
                    }
                    else
                    {
                        await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
                            eventName: "registration_failed",
                            stepCode: "identity",
                            eventStatus: "failure",
                            relatedEntityType: "registration",
                            errorCode: ResponseCodes.CUEREG101_DuplicateMobileNumberOrNickname,
                            errorMessage: await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUEREG101_DuplicateMobileNumberOrNickname),
                            properties: mappedRequest
                        );

                        resp.code = ResponseCodes.Code400_BadRequest;
                        resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                        resp.errors = new List<Errors>
                        {
                            new Errors
                            {
                                errorCode = ResponseCodes.CUEREG101_DuplicateMobileNumberOrNickname,
                                errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUEREG101_DuplicateMobileNumberOrNickname)
                            }
                        };
                        _sqlTransactionHandler.RollbackTransaction();
                        return (HttpStatusCode.BadRequest, resp);
                    }
                }

                // var signUpResponse = await _amazonCognitoMiddleware.SignUpAsync(new SignUpModel.Request
                // {
                //     Username = dbResponse?.cognitoUserName.ToLower(),
                //     Password = Helper.GenerateRandomPassword(12),
                //     AttributeType = new List<SignUpModel.AttributeType>
                //     {
                //         new SignUpModel.AttributeType
                //         {
                //             Name = AttributeTypeName.PhoneNumber,
                //             Value = FormatPhoneNumber(dbResponse?.mobileNumber, countryCode)
                //         },
                //         new SignUpModel.AttributeType
                //         {
                //             Name = AttributeTypeName.Email,
                //             Value = dbResponse?.email
                //         }
                //     }
                // });

                // if (signUpResponse?.Code != ResponseCode.Success)
                // {
                //     await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(), 
                //         eventName: "registration_failed",
                //         stepCode: "identity",
                //         eventStatus: "failure",
                //         relatedEntityType:"registration",
                //         errorCode:ResponseCodes.Code400_BadRequest ,
                //         errorMessage:signUpResponse?.Errors?.FirstOrDefault()?.ErrorMessage,
                //         properties: mappedRequest
                //     );

                //     _sqlTransactionHandler.RollbackTransaction();

                //     resp.code = ResponseCodes.Code400_BadRequest;
                //     resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                //     resp.errors = new List<Errors>
                //     {
                //         new Errors
                //         {
                //             errorCode = ResponseCodes.Code400_BadRequest,
                //             errorMessage = signUpResponse?.Errors?.FirstOrDefault()?.ErrorMessage
                //         }
                //     };
                //     return (HttpStatusCode.BadRequest, resp);
                // }

                await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
                    eventName: "registration_started",
                    stepCode: "identity",
                    eventStatus: "success",
                    relatedEntityType: "customer",
                    relatedEntityId: dbResponse?.holdCustomerId,
                    properties: mappedRequest
                );
                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse.message);
                resp.data = new UserRegistrationResponseModel
                {
                    holdCustomerId = _stringCipher.Encrypt(dbResponse.holdCustomerId),
                    mobileNumber = dbResponse.mobileNumber,
                    processId = dbResponse.processId,
                    expiryTime = Convert.ToInt32(_configuration["UserRegistrationConfirmationCodeExpiryTimeInMilliseconds"])
                };
                _sqlTransactionHandler.CommitTransaction();
                return (HttpStatusCode.OK, resp);
            }
            catch (Exception ex)
            {
                _sqlTransactionHandler.RollbackTransaction();
                throw new Exception("Error occurred.", ex);
            }
        }

        private static string NormalizeDob(string dob)
        {
            if (string.IsNullOrWhiteSpace(dob) || dob == "--")
                return dob;

            return string.Join("-", dob.Split('-')
                .Select(part =>
                    int.TryParse(part, out var num) ? num.ToString("D2") : part
                ));
        }

        public async Task<(HttpStatusCode, object)> UserRegistrationValidationAsync(
            UserRegistrationValidationModel mappedRequest,
            ConversionSessionContext session)
        {
            var validationResult =
                await _validationHelper.ValidateModelAndSignatureAsync(
                    mappedRequest,
                    request => new UserRegistrationValidationModelValidator().Validate(request));

            if (validationResult.Item1 != HttpStatusCode.OK)
                return validationResult;

            var response = new CommonApiResponse();

            if (!string.IsNullOrWhiteSpace(mappedRequest.holdCustomerId))
            {
                mappedRequest.holdCustomerId =
                    _stringCipher.Decrypt(mappedRequest.holdCustomerId);
            }

            var expiryTime =
                _configuration["UserRegistrationConfirmationCodeExpiryTimeInMilliseconds"];

            var holdCustomerDetail = await HoldCustomerDetail(
                mappedRequest.mobileNumber,
                mappedRequest.holdCustomerId,
                mappedRequest.processId,
                "Registration",
                expiryTime);

            if (holdCustomerDetail?.code != "0" ||
                string.IsNullOrWhiteSpace(holdCustomerDetail.cognitoUsername))
            {
                return await BadRequestResponseAsync(
                    holdCustomerDetail?.message);
            }

            var cognitoUsername =
                holdCustomerDetail.cognitoUsername.Trim().ToLowerInvariant();

            var verificationRequest =
                mappedRequest.MapObject<UserRegistrationVerificationRequestCommon>();

            // Check if Cognito user already exists.
            var userDetailResponse =
                await _amazonCognitoMiddleware.GetUserDetailAsync(
                    new UserDetailModel.Request
                    {
                        Username = cognitoUsername
                    });

            if (userDetailResponse?.Code == ResponseCode.Success &&
                userDetailResponse.Data != null)
            {
                var userDetail =
                    userDetailResponse.Data.MapObject<UserDetailModel.Response>();

                verificationRequest.cognitoUserId =
                    userDetail?.Sub ?? string.Empty;
            }

            verificationRequest.actionIP =
                _httpContextAccessor.HttpContext?.GetIpAddress();

            verificationRequest.actionUserId =
                mappedRequest.mobileNumber;

            string? customerAgentId = null;

            var transactionStarted = false;
            var cognitoUserCreated = false;

            try
            {
                _sqlTransactionHandler.BeginTransaction();
                transactionStarted = true;

                var verificationResponse =
                    await _userRegistrationRepository
                        .UserRegistrationVerificationOTP(verificationRequest);

                #region OTP verification validation

                if (verificationResponse?.code != "0" ||
                    string.IsNullOrWhiteSpace(verificationResponse.customerId) ||
                    string.IsNullOrWhiteSpace(verificationResponse.mobileNumber) ||
                    string.IsNullOrWhiteSpace(verificationResponse.password))
                {
                    RollbackTransactionIfRequired(ref transactionStarted);

                    await TrackConversionEvent(
                        session,
                        journeyGuid: GetJourneyGuid(),
                        eventName: "otp_verification_failed",
                        stepCode: "otp",
                        eventStatus: "failure",
                        relatedEntityType: "registration",
                        errorCategory: "customer registration",
                        errorCode: verificationResponse?.code ?? string.Empty,
                        errorMessage: verificationResponse?.message ?? string.Empty,
                        properties: mappedRequest);

                    return await BadRequestResponseAsync(
                        verificationResponse?.message);
                }

                #endregion

                customerAgentId = verificationResponse.customerId;

                var countryCode = _configuration["CountryCode"];

                var formattedMobileNumber =
                    FormatPhoneNumber(
                        mappedRequest.mobileNumber,
                        countryCode);

                #region Duplicate validation

                var duplicateValidationCode =
                    await RemoveUnconfirmedUsersOrGetDuplicateCodeAsync(
                        formattedMobileNumber,
                        cognitoUsername,
                        verificationRequest.actionIP,
                        "CustomerAPI");

                if (!string.IsNullOrWhiteSpace(duplicateValidationCode))
                {
                    RollbackTransactionIfRequired(ref transactionStarted);

                    await TrackConversionEvent(
                        session,
                        journeyGuid: GetJourneyGuid(),
                        eventName: "registration_failed",
                        stepCode: "identity",
                        eventStatus: "failure",
                        relatedEntityType: "customer",
                        errorCategory: "registration",
                        errorCode: duplicateValidationCode,
                        properties: mappedRequest);

                    return await BadRequestResponseAsync(duplicateValidationCode);
                }

                #endregion

                #region Create Cognito user

                var createUserResponse =
                    await _amazonCognitoMiddleware.AdminCreateUserAsync(
                        new common.Models.Middleware.AmazonCognitoModel.SignUp.SignUpModel.Request
                        {
                            Username = cognitoUsername,
                            Password = verificationResponse.password,
                            AttributeType =
                            [
                                new common.Models.Middleware.AmazonCognitoModel.SignUp.SignUpModel.AttributeType
                        {
                            Name = "email",
                            Value = verificationResponse.email
                        },
                        new common.Models.Middleware.AmazonCognitoModel.SignUp.SignUpModel.AttributeType
                        {
                            Name = "phone_number",
                            Value = formattedMobileNumber
                        }
                            ]
                        });

                if (createUserResponse?.Code != ResponseCode.Success)
                {
                    RollbackTransactionIfRequired(ref transactionStarted);

                    var errorMessage =
                        createUserResponse?.Errors?
                            .FirstOrDefault()?
                            .ErrorMessage;

                    await TrackConversionEvent(
                        session,
                        journeyGuid: GetJourneyGuid(),
                        eventName: "registration_failed",
                        stepCode: "identity",
                        eventStatus: "failure",
                        relatedEntityType: "customer",
                        errorCategory: "registration",
                        errorCode: createUserResponse?.Code.ToString() ?? string.Empty,
                        errorMessage: errorMessage ?? string.Empty,
                        properties: mappedRequest);

                    return await BadRequestResponseAsync(errorMessage);
                }

                cognitoUserCreated = true;

                #endregion

                #region Verify mobile number

                var markMobileResponse =
                    await _amazonCognitoMiddleware.MarkMobileNumberAsVerifiedAsync(
                        new MarkMobileNumberAsVerifiedModel.Request
                        {
                            Username = cognitoUsername,
                            MobileNumber = formattedMobileNumber
                        });

                if (markMobileResponse?.Code != ResponseCode.Success)
                {
                    RollbackTransactionIfRequired(ref transactionStarted);

                    await CleanupRegistrationAsync(
                        cognitoUsername,
                        customerAgentId,
                        cognitoUserCreated);

                    var errorMessage =
                        markMobileResponse?.Errors?
                            .FirstOrDefault()?
                            .ErrorMessage;

                    await TrackConversionEvent(
                        session,
                        journeyGuid: GetJourneyGuid(),
                        eventName: "registration_failed",
                        stepCode: "identity",
                        eventStatus: "failure",
                        relatedEntityType: "customer",
                        errorCategory: "registration",
                        errorCode: errorMessage ?? string.Empty,
                        errorMessage: errorMessage ?? string.Empty,
                        properties: mappedRequest);

                    return await BadRequestResponseAsync(errorMessage);
                }

                #endregion

                #region Get Cognito Sub

                var createUserAttributes =
                    createUserResponse.Data.MapObjects<
                        common.Models.Middleware.AmazonCognitoModel.SignUp.SignUpModel
                            .AdminCreateUserResponse>();

                var userSub = createUserAttributes?
                    .FirstOrDefault(x =>
                        string.Equals(
                            x.Name,
                            "sub",
                            StringComparison.OrdinalIgnoreCase))
                    ?.Value;

                if (string.IsNullOrWhiteSpace(userSub))
                {
                    RollbackTransactionIfRequired(ref transactionStarted);

                    await CleanupRegistrationAsync(
                        cognitoUsername,
                        customerAgentId,
                        cognitoUserCreated);

                    var errorCode =
                        createUserResponse.Errors?
                            .FirstOrDefault()?
                            .ErrorMessage
                        ?? ResponseCodes.CUEREG101_DuplicateMobileNumberOrNickname;

                    await TrackConversionEvent(
                        session,
                        journeyGuid: GetJourneyGuid(),
                        eventName: "registration_failed",
                        stepCode: "identity",
                        eventStatus: "failure",
                        relatedEntityType: "customer",
                        errorCategory: "registration",
                        errorCode: errorCode,
                        errorMessage:
                            await _apiResponseMessageService
                                .GetMessageByCodeAsync(
                                    ResponseCodes
                                        .CUEREG101_DuplicateMobileNumberOrNickname),
                        properties: mappedRequest);

                    return await BadRequestResponseAsync(errorCode);
                }

                #endregion

                // All DB operations completed successfully.
                _sqlTransactionHandler.CommitTransaction();
                transactionStarted = false;

                #region Update customer Cognito information

                // Important:
                // Do this BEFORE committing the SQL transaction.
                var updateCustomerResponse =
                    await _reservationRepository.ReservationUpdateCustomerDetail(
                        new ReservationCustomerDetailsUpdateRequestCommon
                        {
                            agentId = customerAgentId,
                            cognitoUserId = userSub
                        });

                if (!IsDbSuccess(updateCustomerResponse?.code))
                {
                    await CleanupRegistrationAsync(
                        cognitoUsername,
                        customerAgentId,
                        cognitoUserCreated);

                    await TrackConversionEvent(
                        session,
                        journeyGuid: GetJourneyGuid(),
                        eventName: "registration_failed",
                        stepCode: "identity",
                        eventStatus: "failure",
                        relatedEntityType: "customer",
                        errorCategory: "registration",
                        errorCode: updateCustomerResponse?.code ?? string.Empty,
                        errorMessage: updateCustomerResponse?.message ?? string.Empty,
                        properties: mappedRequest);

                    return await BadRequestResponseAsync(
                        ResolveMessageCode(updateCustomerResponse?.message));
                }

                #endregion

                await TrackConversionEvent(
                    session,
                    journeyGuid: GetJourneyGuid(),
                    eventName: "identity_resolved",
                    stepCode: "identity",
                    eventStatus: "success",
                    relatedEntityType: "customer",
                    relatedEntityId: customerAgentId,
                    properties: mappedRequest);

                verificationResponse.customerId =
                    _stringCipher.Encrypt(customerAgentId);

                response.code = ResponseCodes.Code200_Success;

                response.message =
                    await _apiResponseMessageService.GetMessageByCodeAsync(
                        verificationResponse.message);

                response.data =
                    verificationResponse
                        .MapObject<UserRegistrationVerificationResponseModel>();

                return (HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                RollbackTransactionIfRequired(ref transactionStarted);

                if (cognitoUserCreated &&
                    !string.IsNullOrWhiteSpace(cognitoUsername) &&
                    !string.IsNullOrWhiteSpace(customerAgentId))
                {
                    try
                    {
                        await HandleRegistrationFailureAsync(
                            cognitoUsername,
                            customerAgentId);
                    }
                    catch
                    {
                    }
                }

                throw new Exception(
                    "An error occurred while validating user registration.",
                    ex);
            }
        }

        public async Task<(HttpStatusCode, object)> CreateUserCredentialsAsync(CreatePasswordModel mappedRequest)
        {
            var validationResult = await _validationHelper.ValidateModelAndSignatureAsync(mappedRequest, req => new CreatePasswordModelValidator().Validate(req));
            if (validationResult.Item1 != HttpStatusCode.OK)
                return validationResult;
            var resp = new CommonApiResponse();
            if (mappedRequest.password != mappedRequest.confirmPassword)
            {
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                resp.errors = new List<Errors>
                {
                    new Errors
                    {
                        errorCode = ResponseCodes.CE464_PasswordAndConfirmPasswordDoesnotMatch,
                        errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CE464_PasswordAndConfirmPasswordDoesnotMatch)
                    }
                };
                return (HttpStatusCode.BadRequest, resp);
            }
            if (!string.IsNullOrEmpty(mappedRequest.customerId))
                mappedRequest.customerId = _stringCipher.Decrypt(mappedRequest.customerId);

            var customerDetailResponse = await _userBusiness.GetCustomerDetail(new shared.User.UserDetailRequestCommon
            {
                customerId = mappedRequest?.customerId,
                mobileNumber = mappedRequest?.mobileNumber,
                processId = mappedRequest?.processId,
                processType = "Setpassword"
            });

            if (customerDetailResponse?.code != "0" || string.IsNullOrEmpty(customerDetailResponse?.cognitoUsername))
            {
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                resp.errors = new List<Errors>
                {
                    new Errors
                    {
                        errorCode = customerDetailResponse?.message,
                        errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(customerDetailResponse?.message)
                    }
                };
                return (HttpStatusCode.BadRequest, resp);
            }

            _sqlTransactionHandler.BeginTransaction();
            try
            {
                var response = await _userRegistrationRepository.UserCreatePasswordAsync(mappedRequest);

                if (response?.code != "0")
                {
                    resp.code = ResponseCodes.Code400_BadRequest;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                    resp.errors = new List<Errors>
                    {
                        new Errors
                        {
                            errorCode = response.message,
                            errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(response.message)
                        }
                    };
                    _sqlTransactionHandler.RollbackTransaction();
                    return (HttpStatusCode.BadRequest, resp);
                }

                var setPasswordResponse = await _amazonCognitoMiddleware.SetPasswordAsync(new SetPasswordModel.Request
                {
                    Username = customerDetailResponse.cognitoUsername.ToLower(),
                    Password = mappedRequest.password,
                    IsPermanent = true
                });

                if (setPasswordResponse?.Code != ResponseCode.Success)
                {
                    resp.code = ResponseCodes.Code400_BadRequest;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                    resp.errors = new List<Errors>
                    {
                        new Errors
                        {
                            errorCode =  ResponseCodes.Code400_BadRequest,
                            errorMessage = setPasswordResponse?.Errors?.FirstOrDefault()?.ErrorMessage
                        }
                    };
                    _sqlTransactionHandler.RollbackTransaction();
                    return (HttpStatusCode.BadRequest, resp);
                }

                response.customerId = _stringCipher.Encrypt(response?.customerId);
                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(response.message);
                resp.data = JsonConvert.DeserializeObject<CreatePasswordResponseModel>(JsonConvert.SerializeObject(response));
                _sqlTransactionHandler.CommitTransaction();
                return (HttpStatusCode.OK, resp);
            }
            catch (Exception ex)
            {
                _sqlTransactionHandler.RollbackTransaction();
                throw new Exception("Error occurred.", ex);
            }
        }
        public async Task<(HttpStatusCode, object)> UserResendOTPAsync(ResendOTPRequestModel mappedRequest)
        {
            var validationResult = await _validationHelper.ValidateModelAndSignatureAsync(mappedRequest, req => new ResendOTPRequestModelValidator().Validate(req));
            if (validationResult.Item1 != HttpStatusCode.OK)
                return validationResult;

            var resp = new CommonApiResponse();

            if (!string.IsNullOrEmpty(mappedRequest.holdCustomerId))
                mappedRequest.holdCustomerId = _stringCipher.Decrypt(mappedRequest.holdCustomerId);

            var commonDBRequest = mappedRequest.MapObject<ResendOTPRequestCommon>();
            commonDBRequest.actionIP = _httpContextAccessor?.HttpContext?.GetIpAddress();
            commonDBRequest.actionUserId = mappedRequest?.mobileNumber;

            _sqlTransactionHandler.BeginTransaction();
            try
            {
                var userResendOTPResponse = await _userRegistrationRepository.UserResendOTPAsync(commonDBRequest);

                if (userResendOTPResponse.code != "0")
                {
                    resp.code = ResponseCodes.Code400_BadRequest;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg400_BadRequest);
                    resp.errors = new List<Errors>
                    {
                        new Errors
                        {
                            errorCode = userResendOTPResponse.message,
                            errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(userResendOTPResponse.message)
                        }
                    };
                    _sqlTransactionHandler.RollbackTransaction();
                    return (HttpStatusCode.BadRequest, resp);
                }

                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(userResendOTPResponse?.message);
                resp.data = new ResendOTPResponseModel
                {
                    holdCustomerId = _stringCipher.Encrypt(userResendOTPResponse?.holdCustomerId),
                    mobileNumber = userResendOTPResponse.mobileNumber,
                    processId = userResendOTPResponse.processId,
                    expiryTime = Convert.ToInt32(_configuration["UserRegistrationConfirmationCodeExpiryTimeInMilliseconds"])
                };
                _sqlTransactionHandler.CommitTransaction();
                return (HttpStatusCode.OK, resp);
            }
            catch (Exception ex)
            {
                _sqlTransactionHandler.RollbackTransaction();
                throw new Exception("Error occurred.", ex);
            }
        }
        public async Task<(HttpStatusCode, object)> ForgotUserPasswordAsync(ForgotPasswordRequestModel mappedRequest)
        {
            var validationResult = await _validationHelper.ValidateModelAndSignatureAsync(mappedRequest, req => new ForgotPasswordRequestModelValidator().Validate(req));
            if (validationResult.Item1 != HttpStatusCode.OK)
                return validationResult;
            var resp = new CommonApiResponse();
            _sqlTransactionHandler.BeginTransaction();
            try
            {
                var response = await _userRegistrationRepository.ForgotUserPasswordAsync(mappedRequest);
                if (response?.code != "0" || string.IsNullOrEmpty(response?.cognitoUsername))
                {
                    resp.code = ResponseCodes.Code400_BadRequest;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                    resp.errors = new List<Errors>
                    {
                        new Errors
                        {
                            errorCode = response.message,
                            errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(response.message)
                        }
                    };
                    _sqlTransactionHandler.RollbackTransaction();
                    return (HttpStatusCode.BadRequest, resp);
                }

                var forgotPasswordResponse = await _amazonCognitoMiddleware.ForgotPasswordAsync(new ForgotPasswordModel.Request
                {
                    Username = response?.cognitoUsername.ToLower()
                });

                if (forgotPasswordResponse.Code != ResponseCode.Success)
                {
                    resp.code = ResponseCodes.Code400_BadRequest;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                    resp.errors = new List<Errors>
                    {
                        new Errors
                        {
                            errorCode = ResponseCodes.Code400_BadRequest,
                            errorMessage = forgotPasswordResponse.Errors?.FirstOrDefault()?.ErrorMessage
                        }
                    };
                    _sqlTransactionHandler.RollbackTransaction();
                    return (HttpStatusCode.BadRequest, resp);
                }

                response.customerId = _stringCipher.Encrypt(response.customerId);
                resp.code = ResponseCodes.Code200_Success;
                resp.message = response.message ?? ResponseMessages.Msg200_Success;
                resp.data = new ForgotPasswordResponseModel
                {
                    customerId = response.customerId,
                    mobileNumber = response.MobileNumber,
                    processId = response.processId,
                    expiryTime = Convert.ToInt32(_configuration["ForgotPasswordConfirmationCodeExpiryTimeInMilliseconds"])
                };
                _sqlTransactionHandler.CommitTransaction();
                return (HttpStatusCode.OK, resp);
            }
            catch (Exception ex)
            {
                _sqlTransactionHandler.RollbackTransaction();
                throw new Exception("Error occurred.", ex);
            }
        }
        public async Task<(HttpStatusCode, object)> ForgotPasswordOTPVerificationAsync(ForgotPasswordOTPVerificatonRequestModel mappedRequest)
        {
            var validationResult = await _validationHelper.ValidateModelAndSignatureAsync(mappedRequest, req => new ForgotPasswordOTPVerificatonRequestModelValidator().Validate(req));
            if (validationResult.Item1 != HttpStatusCode.OK)
                return validationResult;
            var resp = new CommonApiResponse();

            if (!string.IsNullOrEmpty(mappedRequest.customerId))
                mappedRequest.customerId = _stringCipher.Decrypt(mappedRequest.customerId);

            var expiryTime = _configuration["UserRegistrationConfirmationCodeExpiryTimeInMilliseconds"];
            var holdCustomerDetailResponse = await HoldCustomerDetail(mappedRequest.mobileNumber, mappedRequest.customerId, mappedRequest.processId, "ForgotPassword", expiryTime);

            if (holdCustomerDetailResponse?.code != "0" || string.IsNullOrEmpty(holdCustomerDetailResponse?.cognitoUsername))
            {
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                resp.errors = new List<Errors>
                {
                    new Errors
                    {
                        errorCode = holdCustomerDetailResponse?.message,
                        errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(holdCustomerDetailResponse?.message)
                    }
                };
                return (HttpStatusCode.BadRequest, resp);
            }

            _sqlTransactionHandler.BeginTransaction();
            try
            {
                var response = await _userRegistrationRepository.ForgotPasswordOTPVerificationAsync(mappedRequest);
                if (response?.code != "0" || string.IsNullOrEmpty(response?.cognitoUsername))
                {
                    resp.code = ResponseCodes.Code400_BadRequest;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg400_BadRequest);
                    resp.errors = new List<Errors>
                    {
                        new Errors
                        {
                            errorCode = response.message,
                            errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(response.message)
                        }
                    };
                    _sqlTransactionHandler.RollbackTransaction();
                    return (HttpStatusCode.BadRequest, resp);
                }

                var confirmForgotPasswordResponse = await _amazonCognitoMiddleware.ConfirmForgotPasswordAsync(new ConfirmForgotPasswordModel.Request
                {
                    Username = response?.cognitoUsername.ToLower(),
                    ConfirmationCode = mappedRequest?.otpCode,
                    NewPassword = Helper.GenerateRandomPassword(12)
                });

                if (confirmForgotPasswordResponse?.Code != ResponseCode.Success)
                {
                    resp.code = ResponseCodes.Code400_BadRequest;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                    resp.errors = new List<Errors>
                    {
                        new Errors
                        {
                            errorCode = ResponseCodes.Code400_BadRequest,
                            errorMessage = confirmForgotPasswordResponse?.Errors?.FirstOrDefault()?.ErrorMessage
                        }
                    };
                    _sqlTransactionHandler.RollbackTransaction();
                    return (HttpStatusCode.BadRequest, resp);
                }

                response.customerId = _stringCipher.Encrypt(response?.customerId);
                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(response.message);
                resp.data = JsonConvert.DeserializeObject<ForgotPasswordVerificationResponse>(JsonConvert.SerializeObject(response));
                _sqlTransactionHandler.CommitTransaction();
                return (HttpStatusCode.OK, resp);
            }
            catch (Exception ex)
            {
                _sqlTransactionHandler.RollbackTransaction();
                throw new Exception("Error occurred.", ex);
            }
        }
        public async Task<(HttpStatusCode, object)> ForgotPasswordResendOTPAsync(ForgotPasswordResendOTPModel mappedRequest)
        {
            var validationResult = await _validationHelper.ValidateModelAndSignatureAsync(mappedRequest, req => new ForgotPasswordResendOTPModelValidator().Validate(req));
            if (validationResult.Item1 != HttpStatusCode.OK)
                return validationResult;

            var resp = new CommonApiResponse();

            if (!string.IsNullOrEmpty(mappedRequest.customerId))
                mappedRequest.customerId = _stringCipher.Decrypt(mappedRequest.customerId);

            _sqlTransactionHandler.BeginTransaction();
            try
            {
                var response = await _userRegistrationRepository.ForgotPasswordResendOTPAsync(mappedRequest);
                if (response?.code != "0" || string.IsNullOrEmpty(response?.cognitoUsername))
                {
                    resp.code = ResponseCodes.Code400_BadRequest;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                    resp.errors = new List<Errors>
                    {
                        new Errors
                        {
                            errorCode = response.message,
                            errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(response.message)
                        }
                    };
                    _sqlTransactionHandler.RollbackTransaction();
                    return (HttpStatusCode.BadRequest, resp);
                }

                var forgotPasswordResponse = await _amazonCognitoMiddleware.ForgotPasswordAsync(new ForgotPasswordModel.Request
                {
                    Username = response?.cognitoUsername.ToLower()
                });

                if (forgotPasswordResponse.Code != ResponseCode.Success)
                {
                    resp.code = ResponseCodes.Code400_BadRequest;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                    resp.errors = new List<Errors>
                    {
                        new Errors
                        {
                            errorCode = ResponseCodes.Code400_BadRequest,
                            errorMessage = forgotPasswordResponse.Errors?.FirstOrDefault()?.ErrorMessage
                        }
                    };
                    _sqlTransactionHandler.RollbackTransaction();
                    return (HttpStatusCode.BadRequest, resp);
                }

                response.customerId = _stringCipher.Encrypt(response.customerId);
                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(response.message);
                resp.data = new ForgotPasswordResendResponseModel
                {
                    customerId = response.customerId,
                    mobileNumber = response.mobileNumber,
                    processId = response.processId,
                    expiryTime = Convert.ToInt32(_configuration["ForgotPasswordConfirmationCodeExpiryTimeInMilliseconds"])
                };
                _sqlTransactionHandler.CommitTransaction();
                return (HttpStatusCode.OK, resp);
            }
            catch (Exception ex)
            {
                _sqlTransactionHandler.RollbackTransaction();
                throw new Exception("Error occurred.", ex);
            }
        }
        public async Task<(HttpStatusCode, object)> ChangeCustomerPasswordAsync(ChangeCustomerPasswordRequestModel mappedRequest)
        {
            var validationResult = await _validationHelper.ValidateModelAndSignatureAsync(mappedRequest, req => new ChangeCustomerPasswordRequestModelValidator().Validate(req));
            if (validationResult.Item1 != HttpStatusCode.OK)
                return validationResult;

            var resp = new CommonApiResponse();
            var request = mappedRequest.MapObject<CreatePasswordRequestCommon>();
            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(claims => claims.Type == UserClaimsName.UserId)?.Value;
            var agentId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(claims => claims.Type == UserClaimsName.AgentId)?.Value;
            if (request.newPassword != request.confirmPassword)
            {
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                resp.errors = new List<Errors>
                {
                    new Errors
                    {
                        errorCode = ResponseCodes.CUEREG109_NewPasswordAndConfirmPasswordNotMatched,
                        errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUEREG109_NewPasswordAndConfirmPasswordNotMatched)
                    }
                };
                return (HttpStatusCode.BadRequest, resp);
            }
            userId = _stringCipher.Decrypt(userId) ?? string.Empty;
            agentId = _stringCipher.Decrypt(agentId) ?? string.Empty;
            request.actionUserId = userId;

            var customerDetailResponse = await _userBusiness.GetCustomerDetail(new shared.User.UserDetailRequestCommon
            {
                customerId = agentId,
            });

            if (customerDetailResponse?.code != "0" || string.IsNullOrEmpty(customerDetailResponse?.cognitoUsername))
            {
                resp.message = ResponseMessages.Msg400_BadRequest;
                resp.errors = new List<Errors>
                {
                    new Errors{errorCode=customerDetailResponse?.message,errorMessage=await _apiResponseMessageService.GetMessageByCodeAsync(customerDetailResponse?.message)   }
                };
                return (HttpStatusCode.BadRequest, resp);
            }

            if (!hoslog.customer.common.Utilities.SqlServerCrypto.PWDCOMPARE(request?.oldPassword, customerDetailResponse.Password))
            {
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                resp.errors = new List<Errors>
                {
                    new Errors
                    {
                        errorCode = ResponseCodes.CUEREG110_InvalidCurrentPassword,
                        errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUEREG110_InvalidCurrentPassword)
                    }
                };
                return (HttpStatusCode.BadRequest, resp);
            }

            if (hoslog.customer.common.Utilities.SqlServerCrypto.PWDCOMPARE(request?.newPassword, customerDetailResponse.Password))
            {
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                resp.errors = new List<Errors>
                {
                    new Errors
                    {
                        errorCode = ResponseCodes.CUEREG111_NewPasswordCannotBeSameAsCurrentPassword,
                        errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUEREG111_NewPasswordCannotBeSameAsCurrentPassword)
                    }
                };
                return (HttpStatusCode.BadRequest, resp);
            }

            string authorizationHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString();
            var token = authorizationHeader.StartsWith("Bearer ")
                   ? authorizationHeader.Substring("Bearer ".Length).Trim()
                   : null;
            if (string.IsNullOrEmpty(token))
            {
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                return (HttpStatusCode.BadRequest, resp);
            }

            _sqlTransactionHandler.BeginTransaction();
            try
            {
                var response = await _userRegistrationRepository.ChangeCustomerPasswordAsync(request);
                if (response?.code != "0")
                {
                    resp.code = ResponseCodes.Code400_BadRequest;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                    resp.errors = new List<Errors>
                    {
                        new Errors
                        {
                            errorCode = response?.message,
                            errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(response?.message)
                        }
                    };
                    _sqlTransactionHandler.RollbackTransaction();
                    return (HttpStatusCode.BadRequest, resp);
                }

                var changePasswordResponse = await _amazonCognitoMiddleware.ChangePasswordAsync(new ChangePasswordModel.Request
                {
                    AccessToken = token,
                    OldPassword = request.oldPassword,
                    NewPassword = request.newPassword
                });

                if (changePasswordResponse?.Code != ResponseCode.Success)
                {
                    resp.code = ResponseCodes.Code400_BadRequest;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                    resp.errors = new List<Errors>
                    {
                        new Errors
                        {
                            errorCode = ResponseCodes.Code400_BadRequest,
                            errorMessage = changePasswordResponse?.Errors?.FirstOrDefault()?.ErrorMessage
                        }
                    };
                    _sqlTransactionHandler.RollbackTransaction();
                    return (HttpStatusCode.BadRequest, resp);
                }

                await _amazonCognitoMiddleware.SignOutAsync(new SignOutModel.Request
                {
                    AccessToken = token
                });

                response.customerId = _stringCipher.Encrypt(response.customerId);
                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(response.message);
                resp.data = JsonConvert.DeserializeObject<ChangeCustomerPasswordResponseModel>(JsonConvert.SerializeObject(response));
                _sqlTransactionHandler.CommitTransaction();
                return (HttpStatusCode.OK, resp);
            }
            catch (Exception ex)
            {
                _sqlTransactionHandler.RollbackTransaction();
                throw new Exception("Error occurred.", ex);
            }
        }

        public async Task<(HttpStatusCode, object)> ForgotPasswordSetOTPAsync(ForgotSetPasswordModel mappedRequest)
        {
            var validationResult = await _validationHelper.ValidateModelAndSignatureAsync(mappedRequest, req => new ForgotSetPasswordModelValidator().Validate(req));
            if (validationResult.Item1 != HttpStatusCode.OK)
                return validationResult;

            var resp = new CommonApiResponse();
            var request = mappedRequest.MapObject<ForgotSetPasswordCommon>();
            if (string.IsNullOrWhiteSpace(mappedRequest.customerId))
            {
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                resp.errors = new List<Errors>
                {
                    new Errors
                    {
                        errorCode = ResponseCodes.CE465_CustomerIdRequired,
                        errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CE465_CustomerIdRequired)
                    }
                };
                return (HttpStatusCode.BadRequest, resp);
            }
            if (request.password != request.confirmPassword)
            {
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                resp.errors = new List<Errors>
                {
                    new Errors
                    {
                        errorCode = ResponseCodes.CUEREG109_NewPasswordAndConfirmPasswordNotMatched,
                        errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUEREG109_NewPasswordAndConfirmPasswordNotMatched)
                    }
                };
                return (HttpStatusCode.BadRequest, resp);
            }

            request.customerId = _stringCipher.Decrypt(mappedRequest.customerId);
            var customerDetailResponse = await _userBusiness.GetCustomerDetail(new shared.User.UserDetailRequestCommon
            {
                customerId = request?.customerId,
                processId = request?.processId,
                processType = "Setpassword"
            });

            if (customerDetailResponse?.code != "0" || string.IsNullOrEmpty(customerDetailResponse?.cognitoUsername))
            {
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                resp.errors = new List<Errors>
                {
                    new Errors
                    {
                        errorCode = customerDetailResponse?.message,
                        errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(customerDetailResponse?.message)
                    }
                };
                return (HttpStatusCode.BadRequest, resp);
            }

            if (hoslog.customer.common.Utilities.SqlServerCrypto.PWDCOMPARE(request?.password, customerDetailResponse.Password))
            {
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                resp.errors = new List<Errors>
                {
                    new Errors
                    {
                        errorCode = ResponseCodes.CUEREG111_NewPasswordCannotBeSameAsCurrentPassword,
                        errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUEREG111_NewPasswordCannotBeSameAsCurrentPassword)
                    }
                };
                return (HttpStatusCode.BadRequest, resp);
            }

            _sqlTransactionHandler.BeginTransaction();
            try
            {
                var response = await _userRegistrationRepository.ForgotPasswordSetOTPAsync(request);
                if (response.code != "0")
                {
                    resp.code = ResponseCodes.Code400_BadRequest;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                    resp.errors = new List<Errors>
                    {
                        new Errors
                        {
                            errorCode = response.message,
                            errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(response.message)
                        }
                    };
                    _sqlTransactionHandler.RollbackTransaction();
                    return (HttpStatusCode.BadRequest, resp);
                }

                var setPasswordResponse = await _amazonCognitoMiddleware.SetPasswordAsync(new SetPasswordModel.Request
                {
                    Username = response?.cognitoUsername.ToLower(),
                    Password = request.password,
                    IsPermanent = true
                });

                if (setPasswordResponse?.Code != ResponseCode.Success)
                {
                    resp.code = ResponseCodes.Code400_BadRequest;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                    resp.errors = new List<Errors>
                    {
                        new Errors
                        {
                            errorCode = ResponseCodes.Code400_BadRequest,
                            errorMessage = setPasswordResponse?.Errors?.FirstOrDefault()?.ErrorMessage
                        }
                    };
                    _sqlTransactionHandler.RollbackTransaction();
                    return (HttpStatusCode.BadRequest, resp);
                }

                response.customerId = _stringCipher.Encrypt(response.customerId);
                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(response.message);
                resp.data = JsonConvert.DeserializeObject<ForgotSetPasswordResponse>(JsonConvert.SerializeObject(response));
                _sqlTransactionHandler.CommitTransaction();
                return (HttpStatusCode.OK, resp);
            }
            catch (Exception ex)
            {
                _sqlTransactionHandler.RollbackTransaction();
                throw new Exception("Error occurred while updating user details.", ex);
            }
        }

        public async Task<HoldCustomerDetailCommon> HoldCustomerDetail(string mobileNumber, string holdCustomerId, string processId = "", string processType = "", string expiryTime = "")
        {
            return await _userRegistrationRepository.HoldCustomerDetail(mobileNumber, holdCustomerId, processId, processType, expiryTime);
        }

        private string FormatPhoneNumber(string phoneNumber, string countryCode = "+81")
        {
            if (phoneNumber.StartsWith("0"))
                phoneNumber = phoneNumber.Substring(1);
            return $"{countryCode}{phoneNumber}";
        }
        private async Task TrackConversionEvent(
           ConversionSessionContext session,
           Guid? journeyGuid,
           string eventName,
           string stepCode,
           string eventStatus,
           string relatedEntityType,
           string errorCategory = "",
           string errorCode = "",
           string errorMessage = "",
           string relatedEntityId = null,
           object properties = null)
        {
            try
            {
                if (await ShouldBlockConversionFlowAsync(session))
                {
                    _logger.LogInformation($"Conversion flow blocked: Journey is cancelled. Skipping event {eventName}");
                    return;
                }
                if (journeyGuid == null || journeyGuid == Guid.Empty)
                    return;

                await _conversion.InsertEventAsync(new ConversionEventRequestCommon()
                {
                    eventId = Guid.NewGuid(),
                    reservationJourneyId = journeyGuid,
                    conversionSessionSno = _stringCipher.Decrypt(session.conversionSessionSno),
                    eventName = eventName,
                    stepCode = stepCode,
                    eventStatus = eventStatus,
                    eventSource = "backend",
                    actorCustomerId = _stringCipher.Decrypt(session.currentCustomerId),
                    authenticationState = _stringCipher.Decrypt(session.currentCustomerId) != "0" ? "authenticated" : "guest",
                    relatedEntityType = relatedEntityType,
                    relatedEntityId = relatedEntityId,
                    pageUrl = _httpContextAccessor?.HttpContext?.Request.Headers.Origin.ToString(),
                    timezoneOffsetMinutes = _httpContextAccessor?.HttpContext?.Items["TimezoneOffsetMinutes"]?.ToString(),
                    previousPageUrl = _httpContextAccessor?.HttpContext?.Request.Headers.Referer.ToString(),
                    clientUTCDate = DateTime.UtcNow,
                    errorCategory = errorCategory,
                    errorCode = errorCode,
                    errorMessageSafe = errorMessage,
                    propertiesJson = properties != null ? System.Text.Json.JsonSerializer.Serialize(properties) : null,
                    pageType = $"reservation_{stepCode}",
                    requestTraceId = Activity.Current?.TraceId.ToString(),
                });
            }
            catch (Exception ex)
            {
                // Log but don't throw - event tracking should not break the main flow
                Console.WriteLine($"Failed to track conversion event {eventName}: {ex.Message}");
            }
        }

        private Guid? GetJourneyGuid()
        {
            try
            {
                var headerJourneyId = _httpContextAccessor.HttpContext?.Request.Headers["X-Reservation-Journey-Id"].FirstOrDefault();
                if (!string.IsNullOrEmpty(headerJourneyId) && Guid.TryParse(headerJourneyId, out var journeyGuid))
                {
                    return journeyGuid;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private async Task<bool> ShouldBlockConversionFlowAsync(ConversionSessionContext session)
        {
            try
            {
                var journeyGuid = GetJourneyGuid();
                if (journeyGuid == null || journeyGuid == Guid.Empty)
                {
                    return false;
                }

                var journey = await _conversion.GetJourneyAsync(journeyGuid.Value);

                return !ConversionJourneyService.EnsureJourneyCanContinue(
                    _httpContextAccessor.HttpContext!.Request,
                    journey,
                    long.Parse(_stringCipher.Decrypt(session.currentCustomerId)),
                    _logger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check journey status for conversion blocking");
                return false;
            }
        }

        private async Task<string?> RemoveUnconfirmedUsersOrGetDuplicateCodeAsync
        (
            string formattedMobileNumber,
            string cognitoUsername,
            string? actionIp,
            string actionPlatform
        )
        {
            var checkUserExistsResponse =
                await _amazonCognitoMiddleware.CheckUserExistsAsync(formattedMobileNumber);

            var checkUserExistsResponseObject =
                checkUserExistsResponse?.MapObject<
                    common.Models.Middleware.AmazonCognitoModel.UserManagement.CheckAttributeExistsWithDetailsModel.Response>();

            if (checkUserExistsResponseObject?.Users != null)
            {
                foreach (var user in checkUserExistsResponseObject.Users)
                {
                    if (!IsUnconfirmedUser(user?.UserStatus))
                        return ResponseCodes.CUEREG101_DuplicateMobileNumberOrNickname;

                    var deleteResult = await DeleteUnconfirmedUserAsync(
                        user?.Username,
                        actionIp,
                        actionPlatform);

                    if (!deleteResult)
                        return ResponseCodes.CUEREG101_DuplicateMobileNumberOrNickname;
                }
            }

            var getUserDetailResponse =
                await _amazonCognitoMiddleware.GetUserDetailAsync(
                    new UserDetailModel.Request
                    {
                        Username = cognitoUsername
                    });

            if (getUserDetailResponse?.Code == ResponseCode.Success)
            {
                var userDetail =
                    getUserDetailResponse.Data?.MapObject<UserDetailModel.Response>();

                if (IsUnconfirmedUser(userDetail?.UserStatus))
                {
                    var deleteResult = await DeleteUnconfirmedUserAsync(
                        cognitoUsername,
                        actionIp,
                        actionPlatform);

                    if (!deleteResult)
                        return ResponseCodes.CUEREG101_DuplicateMobileNumberOrNickname;
                }
                else
                {
                    return ResponseCodes.CUEREG101_DuplicateMobileNumberOrNickname;
                }
            }

            return null;
        }

        private static bool IsUnconfirmedUser(string? userStatus)
        {
            return string.Equals(
                userStatus,
                "UNCONFIRMED",
                StringComparison.OrdinalIgnoreCase);
        }

        private async Task<bool> DeleteUnconfirmedUserAsync(
        string? username,
        string? actionIp,
        string actionPlatform)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            var adminDeleteResponse =
                await _amazonCognitoMiddleware.AdminDeleteAccountAsync(
                    new common.Models.Middleware.AmazonCognitoModel.UserManagement.AdminDeleteAccountModel.Request
                    {
                        userName = username
                    });

            if (adminDeleteResponse?.Code !=
                hoslog.customer.common.Models.Middleware.AmazonCognito.ResponseCode.Success)
            {
                return false;
            }

            var removeUnconfirmedCustomerInformationResponse =
                await _userRegistrationRepository.RemoveUnconfirmedCustomerInformation(
                    new RemoveUnconfirmedCustomerInformationCommon
                    {
                        nickName = username,
                        actionIP = actionIp,
                        actionPlatform = actionPlatform
                    });

            return removeUnconfirmedCustomerInformationResponse?.Code == ResponseCode.Success;
        }

        private async Task HandleRegistrationFailureAsync(string? cognitoUserName, string? customerId)
        {
            const string url = "reservations/customers";

            var ip = _httpContextAccessor?.HttpContext?.GetIpAddress();

            await ExecuteCleanupStepAsync(
                stepName: "Delete Cognito user",
                identifier: cognitoUserName,
                ip: ip,
                url: url,
                action: async () =>
                {
                    if (string.IsNullOrWhiteSpace(cognitoUserName))
                        return;

                    await _amazonCognitoMiddleware.AdminDeleteAccountAsync(
                        new common.Models.Middleware.AmazonCognitoModel.UserManagement.AdminDeleteAccountModel.Request
                        {
                            userName = cognitoUserName
                        });
                });

            await ExecuteCleanupStepAsync(
                stepName: "Delete DB user",
                identifier: customerId,
                ip: ip,
                url: url,
                action: async () =>
                {
                    if (string.IsNullOrWhiteSpace(customerId))
                        return;

                    await _profileRepository.DeleteUserAsyncWithoutTransaction(
                        new CommonDBRequest
                        {
                            agentId = customerId,
                            actionUserId = "SYSTEM",
                            actionIP = ip,
                            actionPlatform = "WEB"
                        });
                });
        }

        private async Task ExecuteCleanupStepAsync(
        string stepName,
        string? identifier,
        string? ip,
        string url,
        Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                SafeWriteExceptionLog(ex, ip, url);

                Console.WriteLine(
                    $"{stepName} failed. Identifier: {identifier ?? "N/A"}, Error: {ex}");
            }
        }

        private void SafeWriteExceptionLog(Exception ex, string? ip, string url)
        {
            try
            {
                _exceptionLogBusiness.WriteExceptionLog(ex, ip, url);
            }
            catch (Exception logEx)
            {
                Console.WriteLine($"Failed to write exception log. Error: {logEx}");
            }
        }

        private async Task<(HttpStatusCode, object)> BadRequestResponseAsync(
            string messageCode,
            List<Errors>? errors = null
        )
        {
            var resolvedMessageCode = ResolveMessageCode(messageCode);

            return (
                HttpStatusCode.BadRequest,
                CommonApiResponseBuilder.Create()
                    .WithCode(ResponseCodes.Code400_BadRequest)
                    .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(resolvedMessageCode))
                    .WithErrors(errors ?? new List<Errors>())
                    .Build()
            );
        }

        private static string ResolveMessageCode(string? messageCode)
        {
            return string.IsNullOrWhiteSpace(messageCode)
                ? ResponseCodes.Code400_BadRequest
                : messageCode.Trim();
        }

        private static bool IsDbSuccess(string? code)
        {
            return string.Equals(code, "0", StringComparison.Ordinal);
        }

        private void RollbackTransactionIfRequired(ref bool transactionStarted)
        {
            if (!transactionStarted)
                return;

            _sqlTransactionHandler.RollbackTransaction();
            transactionStarted = false;
        }

        private async Task CleanupRegistrationAsync(
            string cognitoUsername,
            string? customerAgentId,
            bool cognitoUserCreated)
        {
            if (!cognitoUserCreated ||
                string.IsNullOrWhiteSpace(customerAgentId))
            {
                return;
            }

            await HandleRegistrationFailureAsync(
                cognitoUsername,
                customerAgentId);
        }
    }
}
