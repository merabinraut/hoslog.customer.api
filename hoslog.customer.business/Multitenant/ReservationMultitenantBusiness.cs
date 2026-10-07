using System.Net;
using System.Text.RegularExpressions;
using hoslog.customer.business.CommonHandler;
using hoslog.customer.common;
using hoslog.customer.common.Middleware.AmazonCognito;
using hoslog.customer.common.Models.Middleware.AmazonCognito;
using hoslog.customer.common.Models.Middleware.AmazonCognitoModel.Auth;
using hoslog.customer.common.Models.Middleware.AmazonCognitoModel.Password;
using hoslog.customer.common.Models.Middleware.AmazonCognitoModel.SignUp;
using hoslog.customer.common.Models.Middleware.AmazonCognitoModel.UserManagement;
using hoslog.customer.common.Models.Multitenant;
using hoslog.customer.common.Service;
using hoslog.customer.common.Utilities;
using hoslog.customer.domain.Multitenant;
using hoslog.customer.domain.Reservation;
using hoslog.customer.domain.UserRegistration;
using hoslog.customer.shared;
using hoslog.customer.shared.Multitenant;
using hoslog.customer.shared.ReservationModel;
using hoslog.customer.shared.UserRegistration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using static hoslog.customer.common.Models.Middleware.AmazonCognitoModel.SignUp.SignUpModel;

namespace hoslog.customer.business.Multitenant
{
    public class ReservationMultitenantBusiness : IReservationMultitenantBusiness
    {
        private readonly IReservationMultitenantRepository _reservationMultitenantRepository;
        private readonly StringCipher _stringCipher;
        private readonly CommonAttributeStringCipher _commonStringCipher;
        private readonly ApiResponseMessageService _apiResponseMessageService;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string[] _baseDomains;
        private readonly string _requestSourceUrl;
        private readonly ValidationHelper _validationHelper;
        private readonly AmazonSettingsCommon _amazonSettingsCommon;
        private readonly IReservationRepository _reservationRepository;
        private readonly LineAPIService _lineAPIService;
        private readonly AmazonCognitoMiddleware _amazonCognitoMiddleware;
        private readonly SqlTransactionHandler _sqlTransactionHandler;
        private readonly IUserRegistrationRepository _userRegistrationRepository;
        public ReservationMultitenantBusiness(IReservationMultitenantRepository reservationMultitenantRepository, StringCipher stringCipher,
            ApiResponseMessageService apiResponseMessageService, IConfiguration configuration, IHttpContextAccessor httpContextAccessor,
            ValidationHelper validationHelper, IReservationRepository reservationRepository, LineAPIService lineAPIService, CommonAttributeStringCipher commonStringCipher,
            AmazonCognitoMiddleware amazonCognitoMiddleware, SqlTransactionHandler sqlTransactionHandler, IUserRegistrationRepository userRegistrationRepository)
        {
            _reservationMultitenantRepository = reservationMultitenantRepository;
            _stringCipher = stringCipher;
            _apiResponseMessageService = apiResponseMessageService;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _baseDomains = _configuration.GetSection("MultitenantBaseDomain").Get<string[]>();
            _requestSourceUrl = GetRequestSourceUrl();
            _validationHelper = validationHelper;
            _amazonSettingsCommon = new AmazonSettingsCommon
            {
                BucketName = _configuration["BucketName"] ?? string.Empty,
                BaseUrl = _configuration["BaseUrl"] ?? string.Empty,
                ConfigAccess = _configuration["ConfigAccess"] ?? string.Empty,
                ConfigSecret = _configuration["ConfigSecret"] ?? string.Empty,
                ImageVirtualPath = _configuration["ImageVirtualPath"] ?? string.Empty,
                PreviousImageUrl = _configuration["PreviousImageUrl"] ?? string.Empty
            };
            _reservationRepository = reservationRepository;
            _lineAPIService = lineAPIService;
            _commonStringCipher = commonStringCipher;
            _amazonCognitoMiddleware = amazonCognitoMiddleware;
            _sqlTransactionHandler = sqlTransactionHandler;
            _userRegistrationRepository = userRegistrationRepository;
        }

        private string GetRequestSourceUrl()
        {
            var headerValue = _httpContextAccessor?.HttpContext?.Items["RequestSourceUrl"] as string;

            if (string.IsNullOrEmpty(headerValue))
            {
                var originHeader = _httpContextAccessor?.HttpContext?.Request.Headers["Origin"].FirstOrDefault();
                var refererHeader = _httpContextAccessor?.HttpContext?.Request.Headers["Referer"].FirstOrDefault();
                headerValue = originHeader ?? refererHeader;
            }
            return headerValue ?? string.Empty;
        }

        public async Task<(HttpStatusCode, CommonApiResponse)> GetAvailableTimeByClub(long? date, string clubId = "")
        {
            var response = new CommonApiResponse
            {
                errors = new List<Errors>()
            };
            clubId = string.IsNullOrEmpty(clubId) ? string.Empty : _stringCipher.Decrypt(clubId);
            var subDomainName = Helper.GetSubdomain(_requestSourceUrl, _baseDomains);
            if (string.IsNullOrEmpty(subDomainName) && string.IsNullOrEmpty(clubId))
            {
                response.code = ResponseCodes.Code400_BadRequest;
                response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUE101_InvalidClub);
                return (HttpStatusCode.BadRequest, response);
            }

            var dbRequest = new ClubReservationTimeRequestMultitenantCommon
            {
                subDomainName = subDomainName,
                clubId = clubId,
                date = date,
                timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty
            };

            var dbResponse = await _reservationMultitenantRepository.GetAvailableTimeByClub(dbRequest);
            var dbResponseFirst = dbResponse?.FirstOrDefault();
            if (dbResponseFirst?.code != "0")
            {
                response.code = ResponseCodes.Code400_BadRequest;
                response.message = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponseFirst?.message) ?? await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUE100_InvalidRequest);
                return (HttpStatusCode.BadRequest, response);
            }

            var timeResponseModel = new ClubReservationTimeResponseMultitenantModel
            {
                date = dbResponseFirst.date,
                clubOpeningTime = dbResponseFirst?.clubOpeningTime ?? string.Empty,
                clubClosingTime = dbResponseFirst?.clubClosingTime ?? string.Empty,
                lastEntryTime = dbResponseFirst?.lastEntryTime ?? string.Empty,
                clubReservationTimeDetailModel = dbResponse?.MapObjects<ClubReservationTimeDetailMultitenantModel>() ?? new List<ClubReservationTimeDetailMultitenantModel>()
            };
            response.code = ResponseCodes.Code200_Success;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse?.FirstOrDefault()?.message) ?? await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code200_Success);
            response.data = timeResponseModel;
            return (HttpStatusCode.OK, response);
        }

        public async Task<(HttpStatusCode, object)> IsValidReservation(string clubId, string noOfPeople, long selectedDateTime)
        {
            var reservationModel = new ConfirmationReservationMultitenantModel()
            {
                clubId = clubId,
                selectedDateTime = selectedDateTime,
                noOfPeople = noOfPeople,
                actionPlatform = _httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("ActionPlatform", out var headerValue) == true ? headerValue.FirstOrDefault() : "web",
            };

            var response = new CommonApiResponse()
            {
                errors = new List<Errors>(),
            };

            var validationResult = await _validationHelper.HandleModelValidationAsync(reservationModel, request => new ConfirmationReservationMultitenantModelValidator().Validate(request));
            if (validationResult.Item1 != HttpStatusCode.OK)
                return validationResult;

            var dbRequest = new ConfirmationReservationMultitenantCommon();
            var _commonClubAndPlan = new CommonClubAndPlanMultitenantModel();
            if (reservationModel != null)
            {
                dbRequest.clubId = string.IsNullOrEmpty(reservationModel.clubId) ? string.Empty : _stringCipher.Decrypt(reservationModel.clubId!);
                dbRequest.noOfPeople = reservationModel.noOfPeople;
                dbRequest.selectedDateTime = reservationModel.selectedDateTime;
                dbRequest.timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty;
                var dbResponse = await _reservationMultitenantRepository.IsValidReservation(dbRequest);
                if (dbResponse.code == "0")
                {
                    var clubDetailsDBResponse = await _reservationRepository.GetClubDetail(dbRequest.clubId);
                    _commonClubAndPlan.clubDetails = clubDetailsDBResponse.MapObjects<ClubBasicDetailMultitenantModel>();
                    ConfirmationReservationMultitenantCommon commonRequest = dbRequest.MapObject<ConfirmationReservationMultitenantCommon>();
                    var planListDBResponse = await _reservationMultitenantRepository.GetPlanList(commonRequest);
                    _commonClubAndPlan.planList = planListDBResponse.MapObjects<PlanMultitenantModel>();
                    var hostListDBResponse = await _reservationMultitenantRepository.GetHostList(dbRequest.clubId);
                    _commonClubAndPlan.hostList = hostListDBResponse.MapObjects<HostMultitenantModel>();
                    // var clubSpecificPlanResponse = await _reservationRepository.GetClubSpecificPlanList(dbRequest.clubId);
                    // _commonClubAndPlan.clubPlanList = clubSpecificPlanResponse.MapObjects<ClubPlanResponseModel>();

                    if (_commonClubAndPlan.planList.Count() > 0 && _commonClubAndPlan.planList[0].code != "1")
                    {
                        // var planListMappedData = _commonClubAndPlan.planList.MapObjects<PlanResponseMultitenantModel>();
                        var clubDetailsMappedData = _commonClubAndPlan.clubDetails.MapObjects<ClubBasicDetailMultitenantModel>();
                        var hostMappedData = _commonClubAndPlan.hostList.MapObjects<HostMultitenantModel>();

                        PlanResponseMultitenantModel MapAndEncryptPlan(PlanResponseMultitenantModel planItem)
                        {
                            if (planItem == null) return null;

                            return new PlanResponseMultitenantModel
                            {
                                planId = _stringCipher.Encrypt(planItem.planId),
                                planName = planItem.planName,
                                planLiquorJapanese = planItem.planLiquorJapanese,
                                isStrikeOut = planItem.isStrikeOut,
                                planLiquor = planItem.planLiquor,
                                planNomination = planItem.planNomination,
                                planPrice = planItem.planPrice,
                                planRemarks = planItem.planRemarks,
                                planTime = planItem.planTime,
                                strikePrice = planItem.strikePrice
                            };
                        }

                        var planListMappedData = _commonClubAndPlan.planList
                           .Where(x => x?.planType != null)
                           .Select(x => new
                           {
                               planType = x.planType.ToLower(),
                               planData = x.MapObject<PlanResponseMultitenantModel>()
                           })
                           .ToList();

                        var adminPlans = planListMappedData
                            .Where(x => x.planType == "adminplan")
                            .Select(x => MapAndEncryptPlan(x.planData))
                            .ToList();

                        var clubPlans = planListMappedData
                            .Where(x => x.planType == "clubplan")
                            .Select(x => MapAndEncryptPlan(x.planData))
                            .ToList();

                        // planListMappedData = _commonClubAndPlan.planList.Select(planItem => new PlanResponseMultitenantModel
                        // {
                        //     planId = _stringCipher.Encrypt(planItem.planId),
                        //     planName = planItem.planName,
                        //     planLiquorJapanese = planItem.planLiquorJapanese,
                        //     isStrikeOut = planItem.isStrikeOut,
                        //     planLiquor = planItem.planLiquor,
                        //     planNomination = planItem.planNomination,
                        //     planPrice = planItem.planPrice,
                        //     planRemarks = planItem.planRemarks,
                        //     planTime = planItem.planTime,
                        //     strikePrice = planItem.strikePrice
                        // }).ToList();
                        clubDetailsMappedData = _commonClubAndPlan.clubDetails.Select(clubDetail => new ClubBasicDetailMultitenantModel
                        {
                            clubId = _stringCipher.Encrypt(clubDetail.clubId),
                            clubLocationName = clubDetail.clubLocationName,
                            clubNameEnglish = clubDetail.clubNameEnglish,
                            clubNameJapanese = clubDetail.clubNameJapanese,
                            clubLogoResponse = new ImageResponse
                            {
                                alt = clubDetail.clubNameEnglish,
                                description = "club logo",
                                path = ImageHandler.ProcessedImage(_amazonSettingsCommon, clubDetail.clubLogo),
                            },
                        }).ToList();
                        hostMappedData = _commonClubAndPlan.hostList.Select(hostItem => new HostMultitenantModel
                        {
                            clubId = _stringCipher.Encrypt(hostItem.clubId),
                            hostId = _stringCipher.Encrypt(hostItem.hostId),
                            hostNameEnglish = hostItem.hostNameEnglish,
                            hostNameJapanese = hostItem.hostNameJapanese,
                            hostPosition = hostItem.hostPosition,
                            hostImageResponse = new ImageResponse
                            {
                                alt = hostItem.hostNameEnglish,
                                description = "host image",
                                path = ImageHandler.ProcessedImage(_amazonSettingsCommon, hostItem.hostImage)
                            }
                        }).ToList();
                        // var respMapped = _commonClubAndPlan.clubPlanList.Select(clubPlans => new ClubPlanResponseModel
                        // {
                        //     planId = _stringCipher.Encrypt(clubPlans.planId.ToString()),
                        //     clubId = _stringCipher.Encrypt(clubPlans.clubId.ToString()),
                        //     planName = clubPlans.planName,
                        //     planPrice = clubPlans.planPrice,
                        //     planTime = clubPlans.planTime,
                        //     noOfNomination = clubPlans.noOfNomination,
                        //     noOfPeople = clubPlans.noOfPeople,
                        // }).ToList();
                        response.code = ResponseCodes.Code200_Success;
                        response.message = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse.message);
                        response.data = new
                        {
                            plans = adminPlans,
                            clubPlans = clubPlans,
                            club_details = clubDetailsMappedData,
                            host_list = hostMappedData
                        };
                    }
                    else
                    {
                        response.code = ResponseCodes.Code400_BadRequest;
                        response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                        response.data = Array.Empty<string>();
                        response.errors = new List<Errors>
                    {
                        new Errors
                        {
                            errorCode = _commonClubAndPlan?.planList?.FirstOrDefault()?.message ?? ResponseCodes.CUERES109_ReservationPlansNotAvailable,
                            errorMessage = string.IsNullOrEmpty(_commonClubAndPlan?.planList?.FirstOrDefault()?.message) ? await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUERES109_ReservationPlansNotAvailable) :
                                            await _apiResponseMessageService.GetMessageByCodeAsync(_commonClubAndPlan.planList.FirstOrDefault().message)
                        }
                    };
                        return (HttpStatusCode.BadRequest, response);
                    }
                }
                else
                {
                    response.code = ResponseCodes.Code400_BadRequest;
                    response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                    response.data = Array.Empty<string>();
                    response.errors = new List<Errors>
                    {
                        new Errors
                        {
                            errorCode = dbResponse.message,
                            errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse.message ?? ResponseCodes.Code400_BadRequest)
                        }
                    };
                    return (HttpStatusCode.BadRequest, response);
                }
            }
            else
            {
                response.code = ResponseCodes.Code400_BadRequest;
                response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                return (HttpStatusCode.OK, response);
            }
            return (HttpStatusCode.OK, response);
        }

        #region Reservation
        public async Task<(HttpStatusCode, object)> ReservationConfirmation(ConfirmReservationMultitenantRequestModel request)
        {
            var response = new CommonApiResponse()
            {
                errors = new List<Errors>()
            };
            var validationResult = await _validationHelper.ValidateModelAndSignatureAsync(request, request => new ConfirmReservationMultitenantRequestValidatorModel().Validate(request));
            if (validationResult.Item1 != HttpStatusCode.OK)
                return validationResult;

            ConfirmReservationMultitenantRequestCommon dbRequest = request.MapObject<ConfirmReservationMultitenantRequestCommon>();
            string[]? hostIdListSplit = dbRequest.hostIdList?.Split(',');
            var hostIdListArray = hostIdListSplit?.Select(hostid => _stringCipher.Decrypt(hostid)).ToArray() ?? Array.Empty<string>();
            var hostIdLists = hostIdListArray != null ? string.Join(",", hostIdListArray.ToArray()) : null;
            dbRequest.planId = string.IsNullOrEmpty(dbRequest.planId) ? string.Empty : _stringCipher.Decrypt(dbRequest.planId);
            dbRequest.clubId = string.IsNullOrEmpty(dbRequest.clubId) ? string.Empty : _stringCipher.Decrypt(dbRequest.clubId);
            dbRequest.paymentType = string.IsNullOrEmpty(dbRequest.paymentType) ? string.Empty : _commonStringCipher.Decrypt(dbRequest.paymentType);
            dbRequest.hostIdList = hostIdLists;
            dbRequest.actionIP = _httpContextAccessor?.HttpContext?.GetIpAddress();
            dbRequest.timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty;
            dbRequest.actionPlatform = _httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("ActionPlatform", out var headerValue) == true ? headerValue.FirstOrDefault() : "web";
            var dbResponse = await _reservationMultitenantRepository.ReservationConfirmation(dbRequest);
            if (dbResponse.code != "0")
            {
                response.code = ResponseCodes.Code400_BadRequest;
                response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg400_BadRequest);
                response.data = Array.Empty<string>();
                response.errors = new List<Errors>
                {
                    new Errors
                    {
                        errorCode = dbResponse.message,
                        errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse?.message)
                    }
                };
                return (HttpStatusCode.BadRequest, response);
            }

            response.code = ResponseCodes.Code200_Success;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse?.message);
            response.data = new ConfirmReservationMultitenantResponseModel
            {
                reservationHoldId = _stringCipher.Encrypt(dbResponse?.reservationHoldId ?? string.Empty),
                reservationTraceId = dbResponse?.reservationTraceId,
                clubCode = dbResponse?.clubCode,
                clubName = dbResponse?.clubName,
                clubNameEng = dbResponse?.clubNameEng,
                clubLogo = new ImageResponse
                {
                    alt = dbResponse?.clubNameEng,
                    description = "club logo",
                    path = string.IsNullOrEmpty(dbResponse?.clubLogo) ? string.Empty : ImageHandler.ProcessedImage(_amazonSettingsCommon, dbResponse?.clubLogo ?? string.Empty)
                }
            };
            return (HttpStatusCode.OK, response);
        }
        #endregion

        #region Get reservation details and send otp
        public async Task<(HttpStatusCode, object)> GetHoldReservationAndSendOTP(GetHoldReservationAndSendOtpRequestModel request)
        {
            var response = new CommonApiResponse()
            {
                errors = new List<Errors>()
            };

            var validationResult = await _validationHelper.ValidateModelAndSignatureAsync(request, request => new GetHoldReservationAndSendOtpRequestValidatorModel().Validate(request));
            if (validationResult.Item1 != HttpStatusCode.OK)
                return validationResult;

            var dbRequest = new GetHoldReservationAndSendOtpRequestCommon
            {
                mobileNumber = request.mobileNumber,
                reservationHoldId = string.IsNullOrEmpty(request.reservationHoldId) ? string.Empty : _stringCipher.Decrypt(request.reservationHoldId),
                reservationTraceId = request.reservationTraceId,
                actionIP = _httpContextAccessor?.HttpContext?.GetIpAddress(),
                actionPlatform = _httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("ActionPlatform", out var headerValue) == true ? headerValue.FirstOrDefault() : "web"
            };

            var dbResponse = await _reservationMultitenantRepository.GetHoldReservationAndSendOTP(dbRequest);
            if (dbResponse.code != "0")
            {
                response.code = ResponseCodes.Code400_BadRequest;
                response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg400_BadRequest);
                response.data = Array.Empty<string>();
                response.errors = new List<Errors>
                {
                    new Errors
                    {
                        errorCode = dbResponse.message,
                        errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse?.message)
                    }
                };
                return (HttpStatusCode.BadRequest, response);
            }

            response.code = ResponseCodes.Code200_Success;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse?.message);
            var responseDataObject = dbResponse.MapObject<GetHoldReservationAndSendOtpResponseModel>();
            responseDataObject.customerId = string.IsNullOrEmpty(responseDataObject.customerId) ? string.Empty : _stringCipher.Encrypt(responseDataObject.customerId);
            responseDataObject.reservationHoldId = string.IsNullOrEmpty(responseDataObject.reservationHoldId) ? string.Empty : _stringCipher.Encrypt(responseDataObject.reservationHoldId);
            responseDataObject.mobileNumber = request.mobileNumber;
            responseDataObject.expiryTime = Convert.ToInt32(_configuration["UserReservationHoldConfirmationCodeExpiryTimeInMilliseconds"]);
            response.data = responseDataObject.MapObject<GetHoldReservationAndSendOtpResponseModel>();
            return (HttpStatusCode.OK, response);
        }

        public async Task<(HttpStatusCode, object)> GetHoldReservationAndReSendResendOTP(GetHoldReservationAndSendOtpRequestModel request)
        {
            var response = new CommonApiResponse()
            {
                errors = new List<Errors>()
            };

            var validationResult = await _validationHelper.ValidateModelAndSignatureAsync(request, request => new GetHoldReservationAndSendOtpRequestValidatorModel().Validate(request));
            if (validationResult.Item1 != HttpStatusCode.OK)
                return validationResult;

            var dbRequest = new GetHoldReservationAndSendOtpRequestCommon
            {
                mobileNumber = request.mobileNumber,
                reservationHoldId = string.IsNullOrEmpty(request.reservationHoldId) ? string.Empty : _stringCipher.Decrypt(request.reservationHoldId),
                reservationTraceId = request.reservationTraceId,
                actionIP = _httpContextAccessor?.HttpContext?.GetIpAddress(),
                actionPlatform = _httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("ActionPlatform", out var headerValue) == true ? headerValue.FirstOrDefault() : "web"
            };

            var dbResponse = await _reservationMultitenantRepository.GetHoldReservationAndResendSendOTP(dbRequest);
            if (dbResponse.code != "0")
            {
                response.code = ResponseCodes.Code400_BadRequest;
                response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg400_BadRequest);
                response.data = Array.Empty<string>();
                response.errors = new List<Errors>
                {
                    new Errors
                    {
                        errorCode = dbResponse.message,
                        errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse?.message)
                    }
                };
                return (HttpStatusCode.BadRequest, response);
            }

            response.code = ResponseCodes.Code200_Success;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse?.message);
            var responseDataObject = dbResponse.MapObject<GetHoldReservationAndSendOtpResponseModel>();
            responseDataObject.customerId = string.IsNullOrEmpty(responseDataObject.customerId) ? string.Empty : _stringCipher.Encrypt(responseDataObject.customerId);
            responseDataObject.reservationHoldId = string.IsNullOrEmpty(responseDataObject.reservationHoldId) ? string.Empty : _stringCipher.Encrypt(responseDataObject.reservationHoldId);
            responseDataObject.mobileNumber = request.mobileNumber;
            responseDataObject.expiryTime = Convert.ToInt32(_configuration["UserReservationHoldConfirmationCodeExpiryTimeInMilliseconds"]);
            response.data = responseDataObject.MapObject<GetHoldReservationAndSendOtpResponseModel>();
            return (HttpStatusCode.OK, response);
        }
        #endregion

        #region Confirm reservation hold
        public async Task<(HttpStatusCode, object)> ConfirmReservationHold(ConfirmReservationHoldMultitenantRequestModel request)
        {
            var response = new CommonApiResponse()
            {
                errors = new List<Errors>()
            };

            var validationResult = await _validationHelper.ValidateModelAndSignatureAsync(request, request => new ConfirmReservationHoldMultitenantRequestValidatorModel().Validate(request));
            if (validationResult.Item1 != HttpStatusCode.OK)
                return validationResult;

            var dbRequest = request.MapObject<ConfirmReservationHoldMultitenantRequestCommon>();
            dbRequest.actionPlatform = _httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("ActionPlatform", out var headerValue) == true ? headerValue.FirstOrDefault() : "web";
            dbRequest.customerId = string.IsNullOrEmpty(dbRequest.customerId) ? string.Empty : _stringCipher.Decrypt(dbRequest.customerId);
            dbRequest.reservationHoldId = string.IsNullOrEmpty(dbRequest.reservationHoldId) ? string.Empty : _stringCipher.Decrypt(dbRequest.reservationHoldId);
            dbRequest.actionIP = _httpContextAccessor?.HttpContext?.GetIpAddress();
            if (string.IsNullOrEmpty(dbRequest.customerId) || string.IsNullOrEmpty(dbRequest.reservationHoldId))
            {
                response.code = ResponseCodes.Code400_BadRequest;
                response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg400_BadRequest);
                response.data = Array.Empty<string>();
                return (HttpStatusCode.BadRequest, response);
            }
            var validateOTPDBRequest = request.MapObject<OTPVerificationMultitenantRequestCommon>();
            validateOTPDBRequest.actionPlatform = dbRequest.actionPlatform;
            validateOTPDBRequest.actionIP = dbRequest.actionIP;
            validateOTPDBRequest.reservationHoldId = dbRequest.reservationHoldId;

            _sqlTransactionHandler.BeginTransaction();
            var validateOTPDBResponse = await _reservationMultitenantRepository.VerifyOTP(validateOTPDBRequest);
            if (validateOTPDBResponse?.code != "0")
            {
                response.code = ResponseCodes.Code400_BadRequest;
                response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg400_BadRequest);
                response.data = Array.Empty<string>();
                response.errors = new List<Errors>
                {
                    new Errors
                    {
                        errorCode = validateOTPDBResponse?.message,
                        errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(validateOTPDBResponse?.message)
                    }
                };
                _sqlTransactionHandler.RollbackTransaction();
                return (HttpStatusCode.OK, response);
            }

            var dbResponse = await _reservationMultitenantRepository.ConfirmReservationHold(dbRequest);
            if (dbResponse?.code == "0")
            {
                response.code = ResponseCodes.Code200_Success;
                response.message = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse?.message);
                if (!string.IsNullOrEmpty(dbResponse.Id))
                {
                    LineGroupAfterReservationCommon obj = new LineGroupAfterReservationCommon
                    {
                        flag = "newresv",
                        clubId = dbResponse.clubId,
                        planId = dbResponse.planId,
                        userName = dbResponse.customerUserName,
                        noOfPeople = dbResponse.noOfPeople,
                        selectedDateTime = dbResponse.visitDateTime,
                        timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty,
                        planAmount = dbResponse.planAmount,
                        planTime = dbResponse.planTime,
                        otpCode = dbResponse.otpCode,
                        reservationId = dbResponse.reservationId
                    };
                    var message = await _reservationRepository.GetMessageToSendInLine(obj);
                    var lineClientSecret = _configuration["LineMessagingClientSecret"] ?? string.Empty;
                    var sendMessageToLinegroup = await _lineAPIService.SendMessageToLineGroup(lineClientSecret, dbResponse?.Id, message);
                }
                _sqlTransactionHandler.CommitTransaction();
                return (HttpStatusCode.OK, response);
            }
            response.code = ResponseCodes.Code400_BadRequest;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg400_BadRequest);
            response.data = Array.Empty<string>();
            response.errors = new List<Errors>
                {
                    new Errors
                    {
                        errorCode = dbResponse.message,
                        errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse?.message)
                    }
                };
            _sqlTransactionHandler.RollbackTransaction();
            return (HttpStatusCode.BadRequest, response);
        }
        #endregion

        #region Verify OTP
        public async Task<(HttpStatusCode, object)> VerifyOTP(OTPVerificationMultitenantRequestModel request)
        {
            var response = new CommonApiResponse()
            {
                errors = new List<Errors>()
            };

            var validationResult = await _validationHelper.ValidateModelAndSignatureAsync(request, request => new OTPVerificationMultitenantRequestValidatorModel().Validate(request));
            if (validationResult.Item1 != HttpStatusCode.OK)
                return validationResult;

            var dbRequest = request.MapObject<OTPVerificationMultitenantRequestCommon>();
            dbRequest.actionPlatform = _httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("ActionPlatform", out var headerValue) == true ? headerValue.FirstOrDefault() : "web";
            dbRequest.actionIP = _httpContextAccessor?.HttpContext?.GetIpAddress();
            dbRequest.reservationHoldId = string.IsNullOrEmpty(dbRequest.reservationHoldId) ? string.Empty : _stringCipher.Decrypt(dbRequest.reservationHoldId);

            if (string.IsNullOrEmpty(dbRequest.reservationHoldId) || string.IsNullOrEmpty(dbRequest.mobileNumber))
            {
                response.code = ResponseCodes.Code400_BadRequest;
                response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg400_BadRequest);
                response.data = Array.Empty<string>();
                return (HttpStatusCode.BadRequest, response);
            }
            _sqlTransactionHandler.BeginTransaction();
            var dbResponse = await _reservationMultitenantRepository.VerifyOTP(dbRequest);
            if (dbResponse?.code == "0")
            {
                response.code = ResponseCodes.Code200_Success;
                response.message = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse?.message);
                _sqlTransactionHandler.CommitTransaction();
                return (HttpStatusCode.OK, response);
            }
            response.code = ResponseCodes.Code400_BadRequest;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg400_BadRequest);
            response.data = Array.Empty<string>();
            response.errors = new List<Errors>
                {
                    new Errors
                    {
                        errorCode = dbResponse.message,
                        errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse?.message)
                    }
                };
            _sqlTransactionHandler.RollbackTransaction();
            return (HttpStatusCode.BadRequest, response);
        }
        #endregion

        #region  Reservation
        public async Task<(HttpStatusCode, object)> ConfirmNonExistingCustomerReservationHold(NonExistingCustomerReservationConfirmMultitenantRequestCommon request)
        {
            var resp = new CommonApiResponse();

            var validationResult = await _validationHelper.ValidateModelAndSignatureAsync(request, req => new NonExistingCustomerReservationConfirmMultitenantRequestValidatorModel().Validate(req));
            if (validationResult.Item1 != HttpStatusCode.OK)
                return validationResult;

            var reservationHoldId = string.IsNullOrEmpty(request.reservationHoldId) ? string.Empty : _stringCipher.Decrypt(request.reservationHoldId);
            if (string.IsNullOrEmpty(reservationHoldId))
            {
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg400_BadRequest);
                return (HttpStatusCode.BadRequest, resp);
            }

            var pattern = @"^(?:\d{4}[-/]{2}|\d{4}[-/]{2}\d{1,2}|\d{4}[-/]\d{1,2}[-/]{2}|[-/]{2}\d{1,2}|-?\d{1,2}[-/]\d{1,2}|\d{4}[-/]\d{1,2}[-/]\d{1,2}|-?\d{1,2}[-/]|[-/]{2})$";

            if (!string.IsNullOrWhiteSpace(request?.dob) &&
                request.dob.Trim() != "--" &&
                !Regex.IsMatch(request.dob.Trim(), pattern))
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
            request.dob = NormalizeDob(request?.dob);

            var registrationDBRequest = request.MapObject<CustomerRegistrationMultitenantRequestCommon>();
            registrationDBRequest.actionIP = _httpContextAccessor?.HttpContext?.GetIpAddress();
            registrationDBRequest.actionPlatform = _httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("ActionPlatform", out var headerValue) == true ? headerValue.FirstOrDefault() : "web";
            registrationDBRequest.reservationHoldId = reservationHoldId;

            _sqlTransactionHandler.BeginTransaction();
            try
            {
                var registrationDBResponse = await _reservationMultitenantRepository.RegisterCustomer(registrationDBRequest);
                if (registrationDBResponse.code != "0")
                {
                    resp.code = ResponseCodes.Code400_BadRequest;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                    resp.errors = new List<Errors>
                    {
                        new Errors
                        {
                            errorCode = registrationDBResponse.message,
                            errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(registrationDBResponse.message)
                        }
                    };
                    _sqlTransactionHandler.RollbackTransaction();
                    return (HttpStatusCode.BadRequest, resp);
                }

                var countryCode = _configuration["CountryCode"];
                var formattedMobileNumber = FormatPhoneNumber(request?.mobileNumber, countryCode);
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

                // Handle the current user details
                var getUserDetailResponse = await _amazonCognitoMiddleware.GetUserDetailAsync(new UserDetailModel.Request
                {
                    Username = registrationDBResponse?.cognitoUsername.ToLower()
                });

                if (getUserDetailResponse?.Code == ResponseCode.Success)
                {
                    var getUserDetailResponseObject = getUserDetailResponse?.Data?.MapObject<UserDetailModel.Response>();
                    if (string.Equals(getUserDetailResponseObject?.UserStatus, "UNCONFIRMED", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!await HandleUnconfirmedUserAsync(request?.nickName, "CustomerAPI"))
                        {
                            return (HttpStatusCode.BadRequest, resp);
                        }
                    }
                }

                var adminCreateUser = await _amazonCognitoMiddleware.AdminCreateUserAsync(new SignUpModel.Request
                {
                    Username = registrationDBResponse?.cognitoUsername.ToLower(),
                    Password = registrationDBResponse?.password,
                    AttributeType = new List<SignUpModel.AttributeType>
                {
                    new AttributeType
                    {
                        Name = "email",
                        Value = request?.emailAddress
                    },
                     new AttributeType
                    {
                        Name = "phone_number",
                        Value =  formattedMobileNumber
                    }
                }
                });

                if (adminCreateUser?.Code != ResponseCode.Success)
                {
                    resp.code = ResponseCodes.Code400_BadRequest;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg400_BadRequest);
                    resp.errors = new List<Errors>
                    {
                        new Errors
                        {
                            errorCode = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg400_BadRequest),
                            errorMessage = adminCreateUser?.Errors?.FirstOrDefault()?.ErrorMessage
                        }
                    };
                    _sqlTransactionHandler.RollbackTransaction();
                    return (HttpStatusCode.BadRequest, resp);
                }

                var markMobileNumberAsVerifiedResponse = await _amazonCognitoMiddleware.MarkMobileNumberAsVerifiedAsync(new MarkMobileNumberAsVerifiedModel.Request
                {
                    Username = registrationDBResponse.cognitoUsername.ToLower(),
                    MobileNumber = formattedMobileNumber
                });

                if (markMobileNumberAsVerifiedResponse?.Code != ResponseCode.Success)
                {
                    resp.code = ResponseCodes.Code400_BadRequest;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                    resp.errors = new List<Errors>
                        {
                            new Errors
                            {
                                errorCode = ResponseCodes.Code400_BadRequest,
                                errorMessage = markMobileNumberAsVerifiedResponse.Errors?.FirstOrDefault()?.ErrorMessage
                            }
                        };
                    _sqlTransactionHandler.RollbackTransaction();
                    return (HttpStatusCode.BadRequest, resp);
                }

                // var markEmailAsVerifiedResponse = await _amazonCognitoMiddleware.MarkEmailAsVerifiedAsync(new MarkEmailAsVerifiedModel.Request
                // {
                //     Username = registrationDBResponse.cognitoUsername.ToLower(),
                //     Email = request?.emailAddress
                // });

                // if (markEmailAsVerifiedResponse?.Code != ResponseCode.Success)
                // {
                //     resp.code = ResponseCodes.Code400_BadRequest;
                //     resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                //     resp.errors = new List<Errors>
                //         {
                //             new Errors
                //             {
                //                 errorCode = ResponseCodes.Code400_BadRequest,
                //                 errorMessage = markEmailAsVerifiedResponse.Errors?.FirstOrDefault()?.ErrorMessage
                //             }
                //         };
                //     _sqlTransactionHandler.RollbackTransaction();
                //     return (HttpStatusCode.BadRequest, resp);
                // }

                var adminCreateUserObject = adminCreateUser.Data.MapObjects<SignUpModel.AdminCreateUserResponse>();
                var userSub = adminCreateUserObject.FirstOrDefault(x => x.Name == "sub")?.Value;
                if (string.IsNullOrEmpty(userSub))
                {
                    resp.code = ResponseCodes.Code400_BadRequest;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg400_BadRequest);
                    resp.errors = new List<Errors>
                    {
                        new Errors
                        {
                            errorCode = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg400_BadRequest),
                            errorMessage = adminCreateUser?.Errors?.FirstOrDefault()?.ErrorMessage
                        }
                    };
                    _sqlTransactionHandler.RollbackTransaction();
                    return (HttpStatusCode.BadRequest, resp);
                }

                var updateCustomerDetailRequest = await _reservationMultitenantRepository.UpdateCustomerDetail(new CustomerDetailUpdateMultitenantRequestCommon
                {
                    agentId = registrationDBResponse?.customerId,
                    cognitoUserId = userSub
                });

                if (updateCustomerDetailRequest?.code != "0")
                {
                    resp.code = ResponseCodes.Code400_BadRequest;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg400_BadRequest);
                    resp.errors = new List<Errors>
                    {
                        new Errors
                        {
                            errorCode = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg400_BadRequest),
                            errorMessage = adminCreateUser?.Errors?.FirstOrDefault()?.ErrorMessage
                        }
                    };
                    _sqlTransactionHandler.RollbackTransaction();
                    return (HttpStatusCode.BadRequest, resp);
                }

                var dbRequest = request.MapObject<ConfirmReservationHoldMultitenantRequestCommon>();
                dbRequest.actionPlatform = _httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("ActionPlatform", out var headerValue1) == true ? headerValue1.FirstOrDefault() : "web";
                dbRequest.customerId = registrationDBResponse?.customerId;
                dbRequest.reservationHoldId = reservationHoldId;
                dbRequest.actionIP = _httpContextAccessor?.HttpContext?.GetIpAddress();
                var dbResponse = await _reservationMultitenantRepository.ConfirmReservationHold(dbRequest);
                if (dbResponse?.code == "0")
                {
                    resp.code = ResponseCodes.Code200_Success;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse?.message);
                    if (!string.IsNullOrEmpty(dbResponse.Id))
                    {
                        LineGroupAfterReservationCommon obj = new LineGroupAfterReservationCommon
                        {
                            flag = "newresv",
                            clubId = dbResponse.clubId,
                            planId = dbResponse.planId,
                            userName = dbResponse.customerUserName,
                            noOfPeople = dbResponse.noOfPeople,
                            selectedDateTime = dbResponse.visitDateTime,
                            timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty,
                            planAmount = dbResponse.planAmount,
                            planTime = dbResponse.planTime,
                            otpCode = dbResponse.otpCode,
                            reservationId = dbResponse.reservationId
                        };
                        var message = await _reservationRepository.GetMessageToSendInLine(obj);
                        var lineClientSecret = _configuration["LineMessagingClientSecret"] ?? string.Empty;
                        var sendMessageToLinegroup = await _lineAPIService.SendMessageToLineGroup(lineClientSecret, dbResponse?.Id, message);
                    }
                    _sqlTransactionHandler.CommitTransaction();
                    return (HttpStatusCode.OK, resp);
                }
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg400_BadRequest);
                resp.data = Array.Empty<string>();
                resp.errors = new List<Errors>
                {
                    new Errors
                    {
                        errorCode = dbResponse.message,
                        errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse?.message)
                    }
                };
                _sqlTransactionHandler.RollbackTransaction();
                return (HttpStatusCode.BadRequest, resp);
            }
            catch (Exception ex)
            {
                _sqlTransactionHandler.RollbackTransaction();
                throw new Exception("Error occurred.", ex);
            }
        }
        #endregion

        private static string NormalizeDob(string dob)
        {
            if (string.IsNullOrWhiteSpace(dob) || dob == "--")
                return dob;

            return string.Join("-", dob.Split('-')
                .Select(part =>
                    int.TryParse(part, out var num) ? num.ToString("D2") : part
                ));
        }

        private string FormatPhoneNumber(string phoneNumber, string countryCode = "+81")
        {
            if (phoneNumber.StartsWith("0"))
                phoneNumber = phoneNumber.Substring(1);
            return $"{countryCode}{phoneNumber}";
        }
    }
}