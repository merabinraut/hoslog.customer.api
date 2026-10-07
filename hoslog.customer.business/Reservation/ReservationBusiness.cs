using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using hoslog.customer.business.CommonHandler;
using hoslog.customer.business.ExceptionLog;
using hoslog.customer.common;
using hoslog.customer.common.Constants;
using hoslog.customer.common.Middleware.AmazonCognito;
using hoslog.customer.common.Models.Auth;
using hoslog.customer.common.Models.Conversion;
using hoslog.customer.common.Models.Middleware.AmazonCognito;
using hoslog.customer.common.Models.Middleware.AmazonCognitoModel.UserManagement;
using hoslog.customer.common.Models.ReservationModel;
using hoslog.customer.common.Service;
using hoslog.customer.common.Utilities;
using hoslog.customer.domain.Auth;
using hoslog.customer.domain.Conversion;
using hoslog.customer.domain.Profile;
using hoslog.customer.domain.Reservation;
using hoslog.customer.domain.UserRegistration;
using hoslog.customer.shared;
using hoslog.customer.shared.Auth;
using hoslog.customer.shared.Conversion;
using hoslog.customer.shared.Reservation;
using hoslog.customer.shared.ReservationModel;
using hoslog.customer.shared.UserRegistration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace hoslog.customer.business.Reservation;

public class ReservationBusiness : IReservationBusiness
{
    private readonly IConversionRepository _conversion;
    private readonly IReservationRepository _repos;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly StringCipher _stringCipher;
    private readonly ValidationHelper _validationHelper;
    private readonly ApiResponseMessageService _apiResponseMessageService;
    private readonly LineAPIService _lineAPIService;
    private readonly IConfiguration _configuration;
    private readonly CommonAttributeStringCipher _commonAttributeStringCipher;
    private readonly AmazonSettingsCommon _amazonSettingsCommon;
    private readonly CognitoOptions _cognitoOptions;
    private readonly ICognitoAuthService _cognitoAuthService;
    private readonly ILogger<ReservationBusiness> _logger;
    private readonly AmazonCognitoMiddleware _amazonCognitoMiddleware;
    private readonly IUserRegistrationRepository _userRegistrationRepository;
    private readonly IAuthRepository _authRepository;
    private readonly string _requestSourceUrl;
    private readonly string[] _baseDomains;
    private readonly SqlTransactionHandler _sqlTransactionHandler;
    private readonly IProfileRepository _profileRepository;
    private readonly IExceptionLogBusiness _exceptionLogBusiness;

    public ReservationBusiness(ILogger<ReservationBusiness> logger, IConversionRepository conversion, IReservationRepository repos, IHttpContextAccessor httpContextAccessor, StringCipher stringCipher, ValidationHelper validationHelper, ApiResponseMessageService apiResponseMessageService, LineAPIService lineAPIService, IConfiguration configuration, CommonAttributeStringCipher commonAttributeStringCipher
        , IOptions<CognitoOptions> cognitoOptions
        , ICognitoAuthService cognitoAuthService
        , AmazonCognitoMiddleware amazonCognitoMiddleware
        , IUserRegistrationRepository userRegistrationRepository
        , IAuthRepository authRepository
        , SqlTransactionHandler sqlTransactionHandler
        , IProfileRepository profileRepository
        , IExceptionLogBusiness exceptionLogBusiness
    )
    {
        _logger = logger;
        _conversion = conversion;
        _repos = repos;
        _httpContextAccessor = httpContextAccessor;
        _stringCipher = stringCipher;
        _validationHelper = validationHelper;
        _apiResponseMessageService = apiResponseMessageService;
        _lineAPIService = lineAPIService;
        _configuration = configuration;
        _commonAttributeStringCipher = commonAttributeStringCipher;
        _amazonSettingsCommon = new AmazonSettingsCommon
        {
            BucketName = _configuration["BucketName"] ?? string.Empty,
            BaseUrl = _configuration["BaseUrl"] ?? string.Empty,
            ConfigAccess = _configuration["ConfigAccess"] ?? string.Empty,
            ConfigSecret = _configuration["ConfigSecret"] ?? string.Empty,
            ImageVirtualPath = _configuration["ImageVirtualPath"] ?? string.Empty,
            PreviousImageUrl = _configuration["PreviousImageUrl"] ?? string.Empty
        };
        _cognitoOptions = cognitoOptions.Value;
        _cognitoAuthService = cognitoAuthService;
        _amazonCognitoMiddleware = amazonCognitoMiddleware;
        _userRegistrationRepository = userRegistrationRepository;
        _authRepository = authRepository;
        _baseDomains = _configuration.GetSection("MultitenantBaseDomain").Get<string[]>();
        _requestSourceUrl = GetRequestSourceUrl();
        _sqlTransactionHandler = sqlTransactionHandler;
        _profileRepository = profileRepository;
        _exceptionLogBusiness = exceptionLogBusiness;
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

    public async Task<(HttpStatusCode, object)> IsValidReservation(string clubId, string userId, string noOfPeople, long selectedDateTime, AmazonSettingsCommon mappedAwsSetting)
    {
        ConfirmationReservationModel reservationModel = new ConfirmationReservationModel()
        {
            clubId = clubId,
            userId = userId,
            selectedDateTime = selectedDateTime,
            noOfPeople = noOfPeople,
            actionPlatform = _httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("ActionPlatform", out var headerValue) == true ? headerValue.FirstOrDefault() : "web",
        };
        var response = new CommonApiResponse()
        {
            errors = new List<Errors>(),
        };
        userId = string.Empty;
        var claims = _httpContextAccessor.HttpContext?.User?.Claims;
        if (claims != null && claims.Count() > 0)
        {
            var claimValue = claims.First(x => x.Type == UserClaimsName.UserId)?.Value;
            userId = string.IsNullOrEmpty(claimValue) ? null : _stringCipher.Decrypt(claimValue);
        }
        else
        {
            response.code = ResponseCodes.Code401_Unauthorized;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg401_Unauthorized);
            return (HttpStatusCode.OK, response);
        }

        var validationResult = await _validationHelper.HandleModelValidationAsync(reservationModel, request => new ConfirmationReservationValidator().Validate(request));
        if (validationResult.Item1 != HttpStatusCode.OK)
            return validationResult;

        var dbRequest = new ConfirmationReservationCommon();
        CommonClubAndPlanModel _commonClubAndPlan = new CommonClubAndPlanModel();
        if (reservationModel != null && !string.IsNullOrEmpty(userId))
        {
            dbRequest.userId = string.IsNullOrEmpty(userId) ? string.Empty : userId;
            dbRequest.clubId = string.IsNullOrEmpty(reservationModel.clubId) ? string.Empty : _stringCipher.Decrypt(reservationModel.clubId!);
            dbRequest.noOfPeople = reservationModel.noOfPeople;
            dbRequest.selectedDateTime = reservationModel.selectedDateTime;
            dbRequest.timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty;
            var dbResponse = await _repos.IsValidReservation(dbRequest);
            if (dbResponse.code == "0")
            {
                //var commonRequest = dbRequest.MapObject<CommonReservationModel>();
                _commonClubAndPlan.clubDetails = await GetClubDetail(dbRequest.clubId);
                ConfirmationReservationCommon commonRequest = dbRequest.MapObject<ConfirmationReservationCommon>();
                _commonClubAndPlan.planList = await GetPlanList(commonRequest);
                _commonClubAndPlan.hostList = await GetHostList(dbRequest.clubId);
                if (_commonClubAndPlan.planList.Count() > 0 && _commonClubAndPlan.planList[0].code != "1")
                {
                    PlanModel MapAndEncryptPlan(PlanModel planItem)
                    {
                        if (planItem == null) return null;

                        return new PlanModel
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
                            planData = x.MapObject<PlanModel>()
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

                    var clubDetailsMappedData = _commonClubAndPlan.clubDetails.MapObjects<ClubBasicDetailModel>();
                    var hostMappedData = _commonClubAndPlan.hostList.MapObjects<HostModel>();
                    clubDetailsMappedData = _commonClubAndPlan.clubDetails.Select(clubDetail => new ClubBasicDetailModel
                    {
                        clubId = _stringCipher.Encrypt(clubDetail.clubId),
                        clubLocationName = clubDetail.clubLocationName,
                        clubNameEnglish = clubDetail.clubNameEnglish,
                        clubNameJapanese = clubDetail.clubNameJapanese,
                        clubLogoResponse = new ImageResponse
                        {
                            alt = clubDetail.clubNameEnglish,
                            description = "club logo",
                            path = ImageHandler.ProcessedImage(mappedAwsSetting, clubDetail.clubLogo),
                        },
                    }).ToList();
                    hostMappedData = _commonClubAndPlan.hostList.Select(hostItem => new HostModel
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
                            path = ImageHandler.ProcessedImage(mappedAwsSetting, hostItem.hostImage)
                        }
                    }).ToList();
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
    private async Task<IList<PlanModel>> GetPlanList(ConfirmationReservationCommon commonModel)
    {
        var dbResponse = await _repos.GetPlanList(commonModel);
        return dbResponse;
    }
    private async Task<IList<ClubBasicDetailModel>> GetClubDetail(string? clubId)
    {
        var dbResponse = await _repos.GetClubDetail(clubId);
        return dbResponse;
    }
    private async Task<IList<HostModel>> GetHostList(string? clubId)
    {
        var dbResponse = await _repos.GetHostList(clubId);
        return dbResponse;
    }
    public async Task<(HttpStatusCode, object)> GetBillingDetails(string? clubId, int? noOfPeople, string? planId)
    {
        var response = new CommonApiResponse()
        {
            errors = new List<Errors>()
        };
        string? userId = string.Empty;
        var claims = _httpContextAccessor.HttpContext?.User?.Claims;
        if (claims != null && claims.Count() > 0)
        {
            var claimValue = claims.First(x => x.Type == UserClaimsName.UserId)?.Value;
            userId = string.IsNullOrEmpty(claimValue) ? null : _stringCipher.Decrypt(claimValue);
        }
        else
        {
            response.code = ResponseCodes.Code401_Unauthorized;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg401_Unauthorized);
            return (HttpStatusCode.OK, response);
        }
        var dbRequest = new BilligDetailRequestModel();
        dbRequest.userId = string.IsNullOrEmpty(userId) ? string.Empty : userId;
        dbRequest.clubId = string.IsNullOrEmpty(clubId) ? "" : _stringCipher.Decrypt(clubId);
        dbRequest.planId = string.IsNullOrEmpty(planId) ? "" : _stringCipher.Decrypt(planId);
        dbRequest.noOfPeople = noOfPeople;
        dbRequest.actionPlatform = _httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("ActionPlatform", out var headerValue) == true ? headerValue.FirstOrDefault() : "web";
        var validationResult = await _validationHelper.HandleModelValidationAsync(dbRequest, request => new BillingDetailRequestModelValidator().Validate(request));
        if (validationResult.Item1 != HttpStatusCode.OK)
            return validationResult;

        var dbResponse = await _repos.GetBillingDetails(dbRequest);
        var agentId = _stringCipher.Decrypt(_httpContextAccessor?.HttpContext?.User?.Claims?.FirstOrDefault(claims => claims.Type == UserClaimsName.AgentId)?.Value) ?? string.Empty;
        var paymentResponse = await GetPaymentMethodList(agentId);
        if (dbResponse != null)
        {
            if (dbResponse.FirstOrDefault()?.code == "0")
            {
                var bdMappedData = dbResponse
                    .MapObjects<BillingDetailsModel>()
                    .Select(bd => new BillingDetailsModel
                    {
                        customerCostAmount = bd.customerCostAmount,
                        customerUserName = bd.customerUserName,
                        reservationRemark = bd.reservationRemark
                    })
                    .ToList();

                // Safely parse customer cost
                long.TryParse(bdMappedData.FirstOrDefault()?.customerCostAmount?.ToString(), out long customerCost);

                var paymentMappedData = paymentResponse
                    .MapObjects<PaymentMethodModel>()
                    .Select(p => new PaymentMethodModel
                    {
                        staticLabel = p.staticLabel,
                        staticLabelEng = p.staticLabelEng,
                        staticLabelJp = p.staticLabelJp,
                        staticValue = p.staticValue,
                        customerPoints = p.customerPoints,
                        allowPayment = p.staticValue != "3" ? true : p.staticValue == "3" && p.customerPoints >= customerCost
                    })
                    .ToList();

                var paymentResponseModel = paymentMappedData
                    .Select(p => new PaymentMethodResponseModel
                    {
                        staticLabel = p.staticLabel,
                        staticLabelEng = p.staticLabelEng,
                        staticLabelJp = p.staticLabelJp,
                        staticValue = !string.IsNullOrEmpty(p.staticValue) ? _stringCipher.Encrypt(p.staticValue) : string.Empty,
                        customerPoints = p.staticValue == "3" ? p.customerPoints.ToString() : string.Empty,
                        allowPayment = p.allowPayment
                    })
                    .ToList();

                response.code = ResponseCodes.Code200_Success;
                response.message = await _apiResponseMessageService
                    .GetMessageByCodeAsync(dbResponse.FirstOrDefault()?.message ?? string.Empty);

                response.data = new
                {
                    billing_details = bdMappedData,
                    payment_method = paymentResponseModel
                };

                return (HttpStatusCode.OK, response);
            }
            else
            {
                response.code = ResponseCodes.Code400_BadRequest;
                response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg400_BadRequest);
                response.data = Array.Empty<string>();
                response.errors = new List<Errors>
                    {
                        new Errors
                        {
                            errorCode = dbResponse.FirstOrDefault()?.message,
                            errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse?.FirstOrDefault()?.message)
                        }
                    };
                return (HttpStatusCode.OK, response);
            }
        }
        else
        {
            response.code = ResponseCodes.Code200_Success;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_RecordNotFound);
            response.data = Array.Empty<string>();
            return (HttpStatusCode.OK, response);
        }
    }

    private async Task<IList<PaymentMethodModel>> GetPaymentMethodList(string agentId = "")
    {
        var dbResponse = await _repos.GetPaymentMethodList(agentId);
        return dbResponse;
    }

    public async Task<(HttpStatusCode, object)> ReservationCheckout(ReservationCheckoutRequestModel dbRequest)
    {
        var response = new CommonApiResponse()
        {
            errors = new List<Errors>()
        };
        string? userId = string.Empty;
        var username = string.Empty; ;
        var claims = _httpContextAccessor.HttpContext?.User?.Claims;
        if (claims != null && claims.Count() > 0)
        {
            var claimValue = claims.First(x => x.Type == UserClaimsName.UserId)?.Value;
            userId = string.IsNullOrEmpty(claimValue) ? null : _stringCipher.Decrypt(claimValue);
            var claimusername = claims.First(x => x.Type == UserClaimsName.Username)?.Value;
            username = string.IsNullOrEmpty(claimusername) ? null : _stringCipher.Decrypt(claimusername);
        }
        else
        {
            response.code = ResponseCodes.Code401_Unauthorized;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg401_Unauthorized);
            return (HttpStatusCode.BadRequest, response);
        }
        dbRequest.actionPlatform = _httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("ActionPlatform", out var headerValue) == true ? headerValue.FirstOrDefault() : "web";
        var validationResult = await _validationHelper.HandleModelValidationAsync(dbRequest, request => new ReservationCheckoutRequestValidatorModel().Validate(request));
        if (validationResult.Item1 != HttpStatusCode.OK)
            return validationResult;

        if (dbRequest != null && !string.IsNullOrEmpty(userId))
        {
            ReservationCheckoutRequestCommon commonDbRequest = new ReservationCheckoutRequestCommon();
            commonDbRequest = dbRequest.MapObject<ReservationCheckoutRequestCommon>();
            string[]? hostIdListSplit = dbRequest.hostIdList?.Split(',');
            var hostIdListArray = hostIdListSplit?.Select(hostid => _stringCipher.Decrypt(hostid)).ToArray() ?? Array.Empty<string>();
            var hostIdLists = hostIdListArray != null ? string.Join(",", hostIdListArray.ToArray()) : null;
            commonDbRequest.userId = string.IsNullOrEmpty(userId) ? string.Empty : userId;
            commonDbRequest.planId = string.IsNullOrEmpty(dbRequest.planId) ? string.Empty : _stringCipher.Decrypt(dbRequest.planId);
            commonDbRequest.clubId = string.IsNullOrEmpty(dbRequest.clubId) ? string.Empty : _stringCipher.Decrypt(dbRequest.clubId);
            commonDbRequest.paymentType = string.IsNullOrEmpty(dbRequest.paymentType) ? string.Empty : _stringCipher.Decrypt(dbRequest.paymentType);
            commonDbRequest.hostIdList = hostIdLists;
            commonDbRequest.actionUser = userId;
            commonDbRequest.actionIP = !string.IsNullOrEmpty(dbRequest.actionIP) ? dbRequest.actionIP : "::1";
            commonDbRequest.timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty;
            var dbResponse = await _repos.ReservationCheckout(commonDbRequest);
            if (dbResponse.code == "0")
            {
                response.code = ResponseCodes.Code200_Success;
                response.message = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse?.message);
                if (!string.IsNullOrEmpty(dbResponse.Id))
                {
                    LineGroupAfterReservationCommon obj = new LineGroupAfterReservationCommon
                    {
                        flag = "newresv",
                        clubId = commonDbRequest.clubId,
                        planId = commonDbRequest.planId,
                        userName = username,
                        noOfPeople = commonDbRequest.noOfPeople,
                        selectedDateTime = commonDbRequest.selectedDateTime,
                        timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty,
                        planAmount = dbResponse.planAmount,
                        planTime = dbResponse.planTime,
                        otpCode = dbResponse.otpCode,
                        reservationId = dbResponse.reservationId,
                    };
                    var message = await _repos.GetMessageToSendInLine(obj);
                    var lineClientSecret = _configuration["LineMessagingClientSecret"] ?? string.Empty;
                    var sendMessageToLinegroup = await _lineAPIService.SendMessageToLineGroup(lineClientSecret, dbResponse?.Id, message);
                }
                return (HttpStatusCode.OK, response);
            }
            else
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
        }
        else
        {
            response.code = ResponseCodes.Code401_Unauthorized;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg401_Unauthorized);
            return (HttpStatusCode.BadRequest, response);
        }
    }

    public async Task<(HttpStatusCode, object)> GetAvailableTimeByClub(string clubId, long? date)
    {
        var response = new CommonApiResponse
        {
            errors = new List<Errors>()
        };
        clubId = string.IsNullOrEmpty(clubId) ? string.Empty : _stringCipher.Decrypt(clubId);
        if (string.IsNullOrEmpty(clubId))
        {
            response.code = ResponseCodes.Code400_BadRequest;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUE101_InvalidClub);
            return (HttpStatusCode.BadRequest, response);
        }

        var dbRequest = new ClubReservationTimeRequestCommon
        {
            clubId = clubId,
            date = date,
            timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty
        };

        var dbResponse = await _repos.GetAvailableTimeByClub(dbRequest);
        var dbResponseFirst = dbResponse?.FirstOrDefault();
        if (dbResponseFirst?.code != "0")
        {
            response.code = ResponseCodes.Code400_BadRequest;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponseFirst?.message) ?? await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUE100_InvalidRequest);
            return (HttpStatusCode.BadRequest, response);
        }

        var timeResponseModel = new ClubReservationTimeResponseModel
        {
            date = dbResponseFirst.date,
            clubOpeningTime = dbResponseFirst?.clubOpeningTime ?? string.Empty,
            clubClosingTime = dbResponseFirst?.clubClosingTime ?? string.Empty,
            lastEntryTime = dbResponseFirst?.lastEntryTime ?? string.Empty,
            clubReservationTimeDetailModel = dbResponse?.MapObjects<ClubReservationTimeDetailModel>() ?? new List<ClubReservationTimeDetailModel>()
        };
        response.code = ResponseCodes.Code200_Success;
        response.message = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse?.FirstOrDefault()?.message) ?? await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code200_Success);
        response.data = timeResponseModel;
        return (HttpStatusCode.OK, response);
    }

    #region Reservation Change Request APIs

    #region Club Public Information

    public async Task<(HttpStatusCode, CommonApiResponse)> GetClubSchedule(string clubId, string clubCode = "", int duration = 1)
    {
        var response = new CommonApiResponse();

        long decryptedClubId = 0;

        if (!string.IsNullOrWhiteSpace(clubId))
        {
            try
            {
                var decryptedValue = _stringCipher.Decrypt(clubId);

                if (!long.TryParse(decryptedValue, out decryptedClubId))
                {
                    decryptedClubId = 0;
                }
            }
            catch
            {
                decryptedClubId = 0;
            }
        }

        if (decryptedClubId <= 0 && string.IsNullOrWhiteSpace(clubCode))
        {
            response.code = ResponseCodes.Code400_BadRequest;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
            response.errors = new List<Errors>
                {
                    new Errors
                    {
                        errorCode = ResponseCodes.CodeCUE101_Invalid_club,
                        errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeCUE101_Invalid_club)
                    }
                };

            return (HttpStatusCode.BadRequest, response);
        }

        var dbRequest = new ReservationScheduleRequestCommon
        {
            clubId = decryptedClubId,
            clubCode = clubCode?.Trim() ?? string.Empty,
            duration = duration,
            timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty
        };

        var dbResponse = await _repos.GetClubSchedule(dbRequest);
        var firstRecord = dbResponse?.FirstOrDefault();

        if (dbResponse == null || dbResponse.Count == 0 || firstRecord == null || firstRecord.code != "0")
        {
            var messageCode = !string.IsNullOrWhiteSpace(firstRecord?.message)
                ? firstRecord.message
                : ResponseCodes.Code400_BadRequest;

            response.code = ResponseCodes.Code400_BadRequest;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(messageCode);

            return (HttpStatusCode.BadRequest, response);
        }

        var mappedData = dbResponse.MapObjects<ReservationScheduleResponseModel>();
        mappedData.ForEach(x => x.eventType = _commonAttributeStringCipher.Encrypt(x?.eventType));

        response.code = ResponseCodes.Code200_Success;
        response.message = !string.IsNullOrWhiteSpace(firstRecord.message)
            ? await _apiResponseMessageService.GetMessageByCodeAsync(firstRecord.message)
            : await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code200_Success);

        response.data = mappedData;
        return (HttpStatusCode.OK, response);
    }

    public async Task<(HttpStatusCode, CommonApiResponse)> GetClubPlanList(string clubId, string clubCode = "", string planId = "")
    {
        var response = new CommonApiResponse();

        long decryptedClubId = 0;

        if (!string.IsNullOrWhiteSpace(clubId))
        {
            try
            {
                var decryptedValue = _stringCipher.Decrypt(clubId);

                if (!long.TryParse(decryptedValue, out decryptedClubId))
                {
                    decryptedClubId = 0;
                }
            }
            catch
            {
                decryptedClubId = 0;
            }
        }

        if (decryptedClubId <= 0 && string.IsNullOrWhiteSpace(clubCode))
        {
            response.code = ResponseCodes.Code400_BadRequest;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
            response.errors = new List<Errors>
                {
                    new Errors
                    {
                        errorCode = ResponseCodes.CodeCUE101_Invalid_club,
                        errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeCUE101_Invalid_club)
                    }
                };

            return (HttpStatusCode.BadRequest, response);
        }

        string decryptedPlanId = string.IsNullOrEmpty(planId) ? string.Empty : _stringCipher.Decrypt(planId);

        string actionUserId = string.Empty;
        var claim = _httpContextAccessor.HttpContext?.User?
            .Claims.FirstOrDefault(c => c.Type == UserClaimsName.UserId)?.Value;

        if (!string.IsNullOrEmpty(claim))
            actionUserId = _stringCipher.Decrypt(claim);

        var dbRequest = new ReservationPlanListRequestCommon
        {
            clubId = decryptedClubId,
            clubCode = clubCode?.Trim() ?? string.Empty,
            actionUserId = actionUserId,
            timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty,
            planId = decryptedPlanId
        };

        var dbResponse = await _repos.GetClubPlanList(dbRequest);
        var firstRecord = dbResponse?.FirstOrDefault();

        if (firstRecord != null && firstRecord.code != "0")
        {
            var messageCode = !string.IsNullOrWhiteSpace(firstRecord.message)
                ? firstRecord.message
                : ResponseCodes.Code400_BadRequest;

            response.code = ResponseCodes.Code400_BadRequest;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(messageCode);

            return (HttpStatusCode.BadRequest, response);
        }

        if (!firstRecord.canReserve)
        {
            var messageCode = !string.IsNullOrWhiteSpace(firstRecord.message)
                            ? firstRecord.message
                            : ResponseCodes.Code200_Success;

            response.code = ResponseCodes.Code200_Success;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(messageCode);
            response.data = new
            {
                canReserve = false,
                nextEligibleDate = firstRecord.nextEligibleDate,
                previousVisitDate = firstRecord.previousVisitDate,
                planData =
                           new
                           {
                               plans = Array.Empty<ReservationPlanListResponseModel>(),
                               clubPlans = Array.Empty<ReservationPlanListResponseModel>()
                           }
            };

            return (HttpStatusCode.OK, response);
        }

        ReservationPlanListResponseModel MapAndEncryptPlan(ReservationPlanListResponseModel planItem)
        {
            if (planItem == null) return null;

            return new ReservationPlanListResponseModel
            {
                planId = _stringCipher.Encrypt(planItem.planId.ToString()),
                planName = planItem.planName,
                planNameEnglish = planItem.planNameEnglish,
                planTime = planItem.planTime,
                planPrice = planItem.planPrice,
                isStrikeOut = planItem.isStrikeOut,
                strikePrice = planItem.strikePrice,
                planNomination = planItem.planNomination,
                planRemarks = planItem.planRemarks,
                planLiquor = planItem.planLiquor,
                planLiquorJapanese = planItem.planLiquorJapanese,
                planType = planItem.planType,
                hasMostPopularTag = planItem.hasMostPopularTag,
                noOfPeople = planItem.noOfPeople
            };
        }

        var mappedData = dbResponse?.MapObjects<ReservationPlanListResponseModel>()
                       ?? new List<ReservationPlanListResponseModel>();

        var adminPlans = mappedData
            .Where(x => x != null && string.Equals(x.planType, "adminplan", StringComparison.OrdinalIgnoreCase))
            .Select(MapAndEncryptPlan)
            .ToList();

        var clubPlans = mappedData
            .Where(x => x != null && string.Equals(x.planType, "clubplan", StringComparison.OrdinalIgnoreCase))
            .Select(MapAndEncryptPlan)
            .ToList();

        response.code = ResponseCodes.Code200_Success;
        response.message = !string.IsNullOrWhiteSpace(firstRecord?.message)
            ? await _apiResponseMessageService.GetMessageByCodeAsync(firstRecord.message)
            : await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code200_Success);

        response.data = new
        {
            canReserve = true,
            planData =
                new
                {
                    plans = adminPlans,
                    clubPlans = clubPlans
                }
        };

        return (HttpStatusCode.OK, response);
    }

    public async Task<(HttpStatusCode, CommonApiResponse)> GetClubTimeslots(string clubId, string clubCode = "", long date = 0, string planId = "")
    {
        var response = new CommonApiResponse();

        long decryptedClubId = 0;

        if (!string.IsNullOrWhiteSpace(clubId))
        {
            try
            {
                var decryptedValue = _stringCipher.Decrypt(clubId);

                if (!long.TryParse(decryptedValue, out decryptedClubId))
                {
                    decryptedClubId = 0;
                }
            }
            catch
            {
                decryptedClubId = 0;
            }
        }

        if (decryptedClubId <= 0 && string.IsNullOrWhiteSpace(clubCode))
        {
            response.code = ResponseCodes.Code400_BadRequest;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
            response.errors = new List<Errors>
                {
                    new Errors
                    {
                        errorCode = ResponseCodes.CodeCUE101_Invalid_club,
                        errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeCUE101_Invalid_club)
                    }
                };

            return (HttpStatusCode.BadRequest, response);
        }

        long decryptedPlanId = 0;

        if (!string.IsNullOrWhiteSpace(planId))
        {
            try
            {
                var decryptedValue = _stringCipher.Decrypt(planId);

                if (!long.TryParse(decryptedValue, out decryptedPlanId))
                {
                    decryptedPlanId = 0;
                }
            }
            catch
            {
                decryptedPlanId = 0;
            }
        }

        if (decryptedPlanId <= 0)
        {
            return (
               HttpStatusCode.BadRequest,
               CommonApiResponseBuilder.Create()
                   .WithCode(ResponseCodes.Code400_BadRequest)
                   .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest))
                   .WithErrors(new List<Errors>
                    {
                            new Errors
                            {
                                errorCode = ResponseCodes.CUERES132_PlanIdRequired,
                                errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUERES132_PlanIdRequired)
                            }
                    })
                    .Build()
            );
        }

        string actionUserId = string.Empty;
        var claim = _httpContextAccessor.HttpContext?.User?
            .Claims.FirstOrDefault(c => c.Type == UserClaimsName.UserId)?.Value;

        if (!string.IsNullOrEmpty(claim))
            actionUserId = _stringCipher.Decrypt(claim);

        var dbRequest = new ReservationTimeslotsRequestCommon
        {
            clubId = decryptedClubId,
            clubCode = clubCode?.Trim() ?? string.Empty,
            actionUserId = actionUserId,
            date = date,
            timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty,
            planId = decryptedPlanId
        };

        var dbResponse = await _repos.GetClubTimeslots(dbRequest);
        var firstRecord = dbResponse?.FirstOrDefault();

        if (firstRecord != null && firstRecord.code != "0")
        {
            var messageCode = !string.IsNullOrWhiteSpace(firstRecord.message)
                ? firstRecord.message
                : ResponseCodes.Code400_BadRequest;

            response.code = ResponseCodes.Code400_BadRequest;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(messageCode);

            return (HttpStatusCode.BadRequest, response);
        }

        var mappedData = dbResponse?.MapObjects<ReservationTimeslotsResponseModel>()
                    ?? new List<ReservationTimeslotsResponseModel>();

        response.code = ResponseCodes.Code200_Success;
        response.message = !string.IsNullOrWhiteSpace(firstRecord?.message)
            ? await _apiResponseMessageService.GetMessageByCodeAsync(firstRecord.message)
            : await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code200_Success);

        response.data = mappedData;

        return (HttpStatusCode.OK, response);
    }

    public async Task<(HttpStatusCode, CommonApiResponse)> GetClubHostList(string clubId = "", string clubCode = "")
    {
        var response = new CommonApiResponse();

        long decryptedClubId = 0;

        if (!string.IsNullOrWhiteSpace(clubId))
        {
            try
            {
                var decryptedValue = _stringCipher.Decrypt(clubId);

                if (!long.TryParse(decryptedValue, out decryptedClubId))
                {
                    decryptedClubId = 0;
                }
            }
            catch
            {
                decryptedClubId = 0;
            }
        }

        if (decryptedClubId <= 0 && string.IsNullOrWhiteSpace(clubCode))
        {
            response.code = ResponseCodes.Code400_BadRequest;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
            response.errors = new List<Errors>
                {
                    new Errors
                    {
                        errorCode = ResponseCodes.CodeCUE101_Invalid_club,
                        errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeCUE101_Invalid_club)
                    }
                };

            return (HttpStatusCode.BadRequest, response);
        }

        var dbResponse = await _repos.GetClubHostList(decryptedClubId, clubCode);
        var firstRecord = dbResponse?.FirstOrDefault();

        if (dbResponse == null || dbResponse.Count == 0 || firstRecord == null || firstRecord.code != "0")
        {
            var messageCode = !string.IsNullOrWhiteSpace(firstRecord?.message)
                ? firstRecord.message
                : ResponseCodes.Code400_BadRequest;

            response.code = ResponseCodes.Code400_BadRequest;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(messageCode);

            return (HttpStatusCode.BadRequest, response);
        }

        var mappedData = dbResponse.Select(hostItem => new ReservationHostListResponseModel
        {
            hostId = _stringCipher.Encrypt(hostItem?.hostId),
            hostNameEnglish = hostItem.hostNameEnglish,
            hostNameJapanese = hostItem.hostNameJapanese,
            hostPosition = hostItem.hostPosition,
            rank = hostItem.rank,
            hostImageResponse = new ImageResponse
            {
                alt = hostItem.hostNameEnglish,
                description = "host image",
                path = ImageHandler.ProcessedImage(_amazonSettingsCommon, hostItem?.hostImage)
            }
        }).ToList();

        response.code = ResponseCodes.Code200_Success;
        response.message = !string.IsNullOrWhiteSpace(firstRecord.message)
            ? await _apiResponseMessageService.GetMessageByCodeAsync(firstRecord.message)
            : await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code200_Success);

        response.data = mappedData;
        return (HttpStatusCode.OK, response);
    }

    #endregion

    #region Reservation Setup

    public async Task<(HttpStatusCode, CommonApiResponse)> GetReservationPreferences()
    {
        var jsonResponse = await _repos.GetReservationPreferences();

        if (string.IsNullOrWhiteSpace(jsonResponse))
        {
            return (HttpStatusCode.BadRequest, new CommonApiResponse
            {
                code = ResponseCodes.Code400_BadRequest,
                message = await _apiResponseMessageService.GetMessageByCodeAsync(
                    ResponseCodes.Code400_BadRequest.ToString())
            });
        }

        var fieldsToEncrypt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "id"
            };

        try
        {
            var encryptedJson = JsonHelper.EncryptFields(
                jsonResponse,
                fieldsToEncrypt,
                raw => _commonAttributeStringCipher.Encrypt(raw)
            );

            var dataNode = JsonNode.Parse(encryptedJson) ?? new JsonObject();

            return (HttpStatusCode.OK, new CommonApiResponse
            {
                code = ResponseCodes.Code200_Success,
                message = await _apiResponseMessageService.GetMessageByCodeAsync(
                    ResponseCodes.Code200_Success.ToString()),
                data = dataNode
            });
        }
        catch (JsonException)
        {
            return (HttpStatusCode.InternalServerError, new CommonApiResponse
            {
                code = ResponseCodes.Code500_InternalServerError,
                message = await _apiResponseMessageService.GetMessageByCodeAsync(
                    ResponseCodes.Code500_InternalServerError.ToString())
            });
        }
    }

    public async Task<(HttpStatusCode, CommonApiResponse)> GetReservationPaymentMethods()
    {
        string actionUserId = string.Empty;
        var claim = _httpContextAccessor.HttpContext?.User?
            .Claims.FirstOrDefault(c => c.Type == UserClaimsName.UserId)?.Value;

        if (!string.IsNullOrEmpty(claim))
            actionUserId = _stringCipher.Decrypt(claim);

        if (string.IsNullOrEmpty(actionUserId))
        {
            return (
                HttpStatusCode.BadRequest,
                CommonApiResponseBuilder.Create()
                    .WithCode(ResponseCodes.Code401_Unauthorized)
                    .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code401_Unauthorized))
                    .Build()
            );
        }

        var dbResponse = await _repos.GetReservationPaymentMethods(actionUserId);

        var mappedResponse = dbResponse?
            .MapObjects<ReservationPaymentMethodsResponseModel>()?
            .Select(x =>
            {
                if (x != null && !string.IsNullOrWhiteSpace(x.value))
                {
                    x.value = _commonAttributeStringCipher.Encrypt(x.value);
                }

                return x;
            })
            .ToList() ?? new List<ReservationPaymentMethodsResponseModel>();

        var message = await _apiResponseMessageService.GetMessageByCodeAsync(
            ResponseCodes.Code200_Success.ToString());

        return (HttpStatusCode.OK,
            CommonApiResponseBuilder.Create()
                .WithCode(ResponseCodes.Code200_Success)
                .WithMessage(message)
                .WithData(mappedResponse)
                .Build());
    }

    #endregion

    #region Reservation Validation and Billing
    public async Task<(HttpStatusCode, CommonApiResponse)> IsReservationValid(
            string clubId = "",
            string clubCode = "",
            string planId = "",
            long selectedDateTime = 0
        )
    {
        long decryptedClubId = 0;

        if (!string.IsNullOrWhiteSpace(clubId))
        {
            try
            {
                var decryptedValue = _stringCipher.Decrypt(clubId);

                if (!long.TryParse(decryptedValue, out decryptedClubId))
                {
                    decryptedClubId = 0;
                }
            }
            catch
            {
                decryptedClubId = 0;
            }
        }

        if (decryptedClubId <= 0 && string.IsNullOrWhiteSpace(clubCode))
        {
            return (
                HttpStatusCode.BadRequest,
                CommonApiResponseBuilder.Create()
                    .WithCode(ResponseCodes.Code400_BadRequest)
                    .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest))
                    .WithErrors(new List<Errors>
                    {
                            new Errors
                            {
                                errorCode = ResponseCodes.CodeCUE101_Invalid_club,
                                errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeCUE101_Invalid_club)
                            }
                    })
                    .Build()
            );
        }

        long decryptedPlanId = 0;

        if (!string.IsNullOrWhiteSpace(planId))
        {
            try
            {
                var decryptedValue = _stringCipher.Decrypt(planId);

                if (!long.TryParse(decryptedValue, out decryptedPlanId))
                {
                    decryptedPlanId = 0;
                }
            }
            catch
            {
                decryptedPlanId = 0;
            }
        }

        if (decryptedPlanId <= 0)
        {
            return (
               HttpStatusCode.BadRequest,
               CommonApiResponseBuilder.Create()
                   .WithCode(ResponseCodes.Code400_BadRequest)
                   .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest))
                   .WithErrors(new List<Errors>
                   {
                                       new Errors
                            {
                                errorCode = ResponseCodes.CUERES132_PlanIdRequired,
                                errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUERES132_PlanIdRequired)
                            },
                            })
                            .Build()
                    );
        }

        if (selectedDateTime <= 0)
        {
            return (
               HttpStatusCode.BadRequest,
               CommonApiResponseBuilder.Create()
                   .WithCode(ResponseCodes.Code400_BadRequest)
                   .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest))
                   .WithErrors(new List<Errors>
                   {
                                       new Errors
                            {
                                errorCode = ResponseCodes.CUERES131_ReservationDateRequired,
                                errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUERES131_ReservationDateRequired)
                            },
                            })
                            .Build()
                    );
        }

        string actionUserId = string.Empty;
        var claim = _httpContextAccessor.HttpContext?.User?
            .Claims.FirstOrDefault(c => c.Type == UserClaimsName.UserId)?.Value;

        if (!string.IsNullOrEmpty(claim))
            actionUserId = _stringCipher.Decrypt(claim);

        var dbRequest = new ReservationValidateRequestCommon
        {
            clubId = decryptedClubId,
            clubCode = clubCode,
            planId = decryptedPlanId,
            selectedDateTime = selectedDateTime,
            actionUserId = actionUserId,
            timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty
        };

        var dbResponse = await _repos.IsReservationValid(dbRequest);
        if (dbResponse == null || dbResponse?.code != "0")
        {
            var messageCode = !string.IsNullOrWhiteSpace(dbResponse?.message)
               ? dbResponse.message
               : ResponseCodes.Code400_BadRequest;

            return (
               HttpStatusCode.BadRequest,
               CommonApiResponseBuilder.Create()
                   .WithCode(ResponseCodes.Code400_BadRequest)
                   .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(messageCode))
                            .Build()
                    );
        }
        var successMessageCode = !string.IsNullOrWhiteSpace(dbResponse?.message)
                       ? dbResponse.message
                       : ResponseCodes.Code200_Success;
        return (HttpStatusCode.OK,
              CommonApiResponseBuilder.Create()
                  .WithCode(ResponseCodes.Code200_Success)
                  .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(successMessageCode))
                           .Build()
                   );
    }

    public async Task<(HttpStatusCode, CommonApiResponse)> GetReservationBilling(
        string clubId = "",
        string clubCode = "",
        string planId = "",
        long visitDateTimeMS = 0,
        int noOfPeople = 0
    )
    {
        string actionUserId = string.Empty;
        var claim = _httpContextAccessor.HttpContext?.User?
            .Claims.FirstOrDefault(c => c.Type == UserClaimsName.UserId)?.Value;

        if (!string.IsNullOrEmpty(claim))
            actionUserId = _stringCipher.Decrypt(claim);

        if (string.IsNullOrEmpty(actionUserId))
        {
            return (
                HttpStatusCode.BadRequest,
                CommonApiResponseBuilder.Create()
                    .WithCode(ResponseCodes.Code401_Unauthorized)
                    .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code401_Unauthorized))
                    .Build()
            );
        }

        long decryptedClubId = 0;

        if (!string.IsNullOrWhiteSpace(clubId))
        {
            try
            {
                var decryptedValue = _stringCipher.Decrypt(clubId);

                if (!long.TryParse(decryptedValue, out decryptedClubId))
                {
                    decryptedClubId = 0;
                }
            }
            catch
            {
                decryptedClubId = 0;
            }
        }

        if (decryptedClubId <= 0 && string.IsNullOrWhiteSpace(clubCode))
        {
            return (
                HttpStatusCode.BadRequest,
                CommonApiResponseBuilder.Create()
                    .WithCode(ResponseCodes.Code400_BadRequest)
                    .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest))
                    .WithErrors(new List<Errors>
                    {
                            new Errors
                            {
                                errorCode = ResponseCodes.CodeCUE101_Invalid_club,
                                errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeCUE101_Invalid_club)
                            }
                    })
                    .Build()
            );
        }

        long decryptedPlanId = 0;

        if (!string.IsNullOrWhiteSpace(planId))
        {
            try
            {
                var decryptedValue = _stringCipher.Decrypt(planId);

                if (!long.TryParse(decryptedValue, out decryptedPlanId))
                {
                    decryptedPlanId = 0;
                }
            }
            catch
            {
                decryptedPlanId = 0;
            }
        }

        if (decryptedPlanId <= 0)
        {
            return (
               HttpStatusCode.BadRequest,
               CommonApiResponseBuilder.Create()
                   .WithCode(ResponseCodes.Code400_BadRequest)
                   .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest))
                   .WithErrors(new List<Errors>
                   {
                                       new Errors
                            {
                                errorCode = ResponseCodes.CUERES132_PlanIdRequired,
                                errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUERES132_PlanIdRequired)
                            },
                            })
                            .Build()
                    );
        }

        if (noOfPeople <= 0)
        {
            return (
               HttpStatusCode.BadRequest,
               CommonApiResponseBuilder.Create()
                   .WithCode(ResponseCodes.Code400_BadRequest)
                   .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest))
                   .WithErrors(new List<Errors>
                   {
                                       new Errors
                            {
                                errorCode = ResponseCodes.CUERES129_NumberOfPeopleRequired,
                                errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUERES129_NumberOfPeopleRequired)
                            },
                            })
                            .Build()
                    );
        }

        if (visitDateTimeMS <= 0)
        {
            return (
               HttpStatusCode.BadRequest,
               CommonApiResponseBuilder.Create()
                   .WithCode(ResponseCodes.Code400_BadRequest)
                   .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest))
                   .WithErrors(new List<Errors>
                   {
                                       new Errors
                            {
                                errorCode = ResponseCodes.CUERES131_ReservationDateRequired,
                                errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUERES131_ReservationDateRequired)
                            },
                            })
                            .Build()
                    );
        }

        var dbRequest = new ReservationBillingRequestCommon
        {
            clubId = decryptedClubId,
            clubCode = clubCode,
            planId = decryptedPlanId,
            noOfPeople = noOfPeople,
            actionUserId = actionUserId,
            timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty
        };

        var dbReservationValidationRequest = new ReservationValidateRequestCommon
        {
            clubId = dbRequest.clubId,
            planId = dbRequest.planId,
            selectedDateTime = visitDateTimeMS,
            actionUserId = dbRequest.actionUserId,
            timeZoneOffsetValue = dbRequest.timeZoneOffsetValue
        };

        var dbReservationValidationResponse = await _repos.IsReservationValid(dbReservationValidationRequest);
        if (dbReservationValidationResponse == null || dbReservationValidationResponse?.code != "0")
        {
            var messageCode = !string.IsNullOrWhiteSpace(dbReservationValidationResponse?.message)
               ? dbReservationValidationResponse.message
               : ResponseCodes.Code400_BadRequest;

            return (
               HttpStatusCode.BadRequest,
               CommonApiResponseBuilder.Create()
                   .WithCode(ResponseCodes.Code400_BadRequest)
                   .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(messageCode))
                            .Build()
                    );
        }

        var dbResponse = await _repos.GetReservationBilling(dbRequest);
        if (dbResponse == null || dbResponse?.code != "0")
        {
            var messageCode = !string.IsNullOrWhiteSpace(dbResponse?.message)
               ? dbResponse.message
               : ResponseCodes.Code400_BadRequest;

            return (
               HttpStatusCode.BadRequest,
               CommonApiResponseBuilder.Create()
                   .WithCode(ResponseCodes.Code400_BadRequest)
                   .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(messageCode))
                            .Build()
                    );
        }

        var mappedResponse = dbResponse?
          .MapObject<ReservationBillingResponseModel>();

        var successMessageCode = !string.IsNullOrWhiteSpace(dbResponse?.message)
                       ? dbResponse.message
                       : ResponseCodes.Code200_Success;
        return (HttpStatusCode.OK,
              CommonApiResponseBuilder.Create()
                  .WithCode(ResponseCodes.Code200_Success)
                  .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(successMessageCode))
                  .WithData(mappedResponse)
                  .Build()
                   );
    }

    #endregion

    #region Reservation Confirmation
    public async Task<(HttpStatusCode, object)> ConfirmReservation(ReservationConfirmRequestModel request, ConversionSessionContext session)
    {
        string actionUserId = string.Empty;
        string username = string.Empty;

        var claimUserId = _httpContextAccessor.HttpContext?.User?
            .Claims.FirstOrDefault(c => c.Type == UserClaimsName.UserId)?.Value;

        var claimusername = _httpContextAccessor.HttpContext?.User?
            .Claims.FirstOrDefault(c => c.Type == UserClaimsName.Username)?.Value;

        if (!string.IsNullOrEmpty(claimUserId))
            actionUserId = _stringCipher.Decrypt(claimUserId);

        if (!string.IsNullOrEmpty(claimusername))
            username = _stringCipher.Decrypt(claimusername);

        var validationResult = await _validationHelper.HandleModelValidationAsync(request, request => new ReservationConfirmRequestValidatorModel().Validate(request));
        if (validationResult.Item1 != HttpStatusCode.OK)
            return validationResult;



        if (string.IsNullOrEmpty(actionUserId) || string.IsNullOrEmpty(username))
        {
            return (
                HttpStatusCode.BadRequest,
                CommonApiResponseBuilder.Create()
                    .WithCode(ResponseCodes.Code401_Unauthorized)
                    .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code401_Unauthorized))
                    .Build()
            );
        }

        long decryptedClubId = 0;

        if (!string.IsNullOrWhiteSpace(request.clubId))
        {
            try
            {
                var decryptedValue = _stringCipher.Decrypt(request.clubId);

                if (!long.TryParse(decryptedValue, out decryptedClubId))
                {
                    decryptedClubId = 0;
                }
            }
            catch
            {
                decryptedClubId = 0;
            }
        }

        if (decryptedClubId <= 0)
        {
            return (
                HttpStatusCode.BadRequest,
                CommonApiResponseBuilder.Create()
                    .WithCode(ResponseCodes.Code400_BadRequest)
                    .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest))
                    .WithErrors(new List<Errors>
                    {
                            new Errors
                            {
                                errorCode = ResponseCodes.CodeCUE101_Invalid_club,
                                errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeCUE101_Invalid_club)
                            }
                    })
                    .Build()
            );
        }

        long decryptedPlanId = 0;

        if (!string.IsNullOrWhiteSpace(request.planId))
        {
            try
            {
                var decryptedValue = _stringCipher.Decrypt(request.planId);

                if (!long.TryParse(decryptedValue, out decryptedPlanId))
                {
                    decryptedPlanId = 0;
                }
            }
            catch
            {
                decryptedPlanId = 0;
            }
        }

        if (decryptedPlanId <= 0)
        {
            return (
               HttpStatusCode.BadRequest,
               CommonApiResponseBuilder.Create()
                   .WithCode(ResponseCodes.Code400_BadRequest)
                   .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest))
                   .WithErrors(new List<Errors>
                   {
                                       new Errors
                            {
                                errorCode = ResponseCodes.CUERES132_PlanIdRequired,
                                errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUERES132_PlanIdRequired)
                            },
                            })
                            .Build()
                    );
        }

        if (request.noOfPeople <= 0)
        {
            return (
               HttpStatusCode.BadRequest,
               CommonApiResponseBuilder.Create()
                   .WithCode(ResponseCodes.Code400_BadRequest)
                   .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest))
                   .WithErrors(new List<Errors>
                   {
                                       new Errors
                            {
                                errorCode = ResponseCodes.CUERES129_NumberOfPeopleRequired,
                                errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUERES129_NumberOfPeopleRequired)
                            },
                            })
                            .Build()
                    );
        }

        if (request.visitDateTimeMS <= 0)
        {
            return (
               HttpStatusCode.BadRequest,
               CommonApiResponseBuilder.Create()
                   .WithCode(ResponseCodes.Code400_BadRequest)
                   .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest))
                   .WithErrors(new List<Errors>
                   {
                                       new Errors
                            {
                                errorCode = ResponseCodes.CUERES131_ReservationDateRequired,
                                errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUERES131_ReservationDateRequired)
                            },
                            })
                            .Build()
                    );
        }

        long decryptedReservationHoldId = 0;
        if (!string.IsNullOrEmpty(request.reservationHoldId))
        {
            if (!TryDecryptLong(request.reservationHoldId, out decryptedReservationHoldId) ||
                decryptedReservationHoldId <= 0)
                return await ValidationErrorResponseAsync("Invalid reservation hold id.");

            var holdDetailResponse =
           await _repos.ReservationHoldConfirmedDetails(decryptedReservationHoldId);

            if (!IsDbSuccess(holdDetailResponse?.code))
            {
                return await BadRequestResponseAsync(
                    ResolveMessageCode(holdDetailResponse?.message)
                );
            }
        }

        string[]? hostIdListSplit = request.hostIds?.Split(',');
        var hostIdListArray = hostIdListSplit?.Select(hostid => _stringCipher.Decrypt(hostid)).ToArray() ?? Array.Empty<string>();
        var hostIdLists = hostIdListArray != null ? string.Join(",", hostIdListArray.ToArray()) : null;

        string decryptedCustomerPreferencesJson = string.Empty;
        if (!string.IsNullOrWhiteSpace(request.customerPreferencesJson))
        {

            var fieldsToDecrypt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "preferenceOptionIds"
                };

            try
            {
                var decryptedJson = JsonHelper.DecryptFields(
                    request.customerPreferencesJson,
                    fieldsToDecrypt,
                    raw => _commonAttributeStringCipher.Decrypt(raw), // FIXED: use Decrypt, not Encrypt
                    convertNumericStringToNumber: true
                );

                decryptedCustomerPreferencesJson = JsonNode.Parse(decryptedJson)?.ToJsonString(new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = false
                }) ?? string.Empty;
            }
            catch (JsonException)
            {
                decryptedCustomerPreferencesJson = string.Empty;
            }
        }

        var dbRequest = new ReservationConfirmRequestCommon
        {
            clubId = decryptedClubId,
            noOfPeople = request.noOfPeople,
            hostIds = hostIdLists,
            planId = decryptedPlanId,
            paymentType = string.IsNullOrEmpty(request?.paymentType) ? string.Empty : _commonAttributeStringCipher.Decrypt(request.paymentType),
            visitDateTimeMS = request.visitDateTimeMS,
            timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString(),
            actionIP = _httpContextAccessor?.HttpContext?.GetIpAddress(),
            actionPlatform = _httpContextAccessor?.HttpContext?.Request.Headers.TryGetValue("ActionPlatform", out var headerValue) == true ? headerValue.FirstOrDefault() : "web",
            actionUserId = actionUserId,
            customerPreferencesJson = decryptedCustomerPreferencesJson,
            reservationHoldId = string.IsNullOrEmpty(request.reservationHoldId) ? 0 : decryptedReservationHoldId
        };

        var dbReservationValidationRequest = new ReservationValidateRequestCommon
        {
            clubId = dbRequest.clubId,
            planId = dbRequest.planId,
            selectedDateTime = dbRequest.visitDateTimeMS,
            actionUserId = dbRequest.actionUserId,
            timeZoneOffsetValue = dbRequest.timeZoneOffsetValue
        };

        var dbReservationValidationResponse = await _repos.IsReservationValid(dbReservationValidationRequest);
        if (dbReservationValidationResponse == null || dbReservationValidationResponse?.code != "0")
        {
            var messageCode = !string.IsNullOrWhiteSpace(dbReservationValidationResponse?.message)
               ? dbReservationValidationResponse.message
               : ResponseCodes.Code400_BadRequest;

            return (
               HttpStatusCode.BadRequest,
               CommonApiResponseBuilder.Create()
                   .WithCode(ResponseCodes.Code400_BadRequest)
                   .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(messageCode))
                            .Build()
                    );
        }

        var dbResponse = await _repos.ConfirmReservationAsync(dbRequest);
        if (dbResponse == null || dbResponse?.code != "0")
        {
            var messageCode = !string.IsNullOrWhiteSpace(dbResponse?.message)
               ? dbResponse.message
               : ResponseCodes.Code400_BadRequest;

            return (
               HttpStatusCode.BadRequest,
               CommonApiResponseBuilder.Create()
                   .WithCode(ResponseCodes.Code400_BadRequest)
                   .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(messageCode))
                            .Build()
                    );

        }

        await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
            eventName: "reservation_completed",
            stepCode: "confirmation",
            eventStatus: "success",
            relatedEntityType: "reservation",
            relatedEntityId: dbResponse.reservationId,
            properties: new
            {
                reservationId = dbResponse.reservationId,
                clubId = dbRequest.clubId,
                planId = dbRequest.planId,
                noOfPeople = request.noOfPeople
            });

        if (!string.IsNullOrEmpty(dbResponse.lineGroupId))
        {
            LineGroupAfterReservationCommon obj = new LineGroupAfterReservationCommon
            {
                flag = "newresv",
                clubId = dbRequest.clubId.ToString(),
                planId = dbRequest.planId.ToString(),
                userName = username,
                noOfPeople = request.noOfPeople.ToString(),
                selectedDateTime = request.visitDateTimeMS,
                timeZoneOffsetValue = dbRequest.timeZoneOffsetValue,
                planAmount = dbResponse.planAmount.ToString(),
                planTime = dbResponse.planTime.ToString(),
                otpCode = dbResponse.otpCode,
                reservationId = dbResponse.reservationId,
            };
            var message = await _repos.GetMessageToSendInLine(obj);
            var lineClientSecret = _configuration["LineMessagingClientSecret"] ?? string.Empty;
            var sendMessageToLinegroup = await _lineAPIService.SendMessageToLineGroup(lineClientSecret, dbResponse?.lineGroupId, message);
        }


        var successMessageCode = !string.IsNullOrWhiteSpace(dbResponse?.message)
                   ? dbResponse.message
                   : ResponseCodes.Code200_Success;
        return (HttpStatusCode.OK,
              CommonApiResponseBuilder.Create()
                  .WithCode(ResponseCodes.Code200_Success)
                  .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(successMessageCode))
                  .Build()
                   );
    }
    #endregion


    #region Reservation Hold Authentication

    public async Task<(HttpStatusCode, object)> ConfirmReservationHold(ReservationHoldConfirmRequestModel request, ConversionSessionContext session)
    {
        var validationResponse = await _validationHelper.ValidateModelAndSignatureAsync(
            request,
            x => new ReservationHoldConfirmRequestValidatorModel().Validate(x)
        );

        if (validationResponse.Item1 != HttpStatusCode.OK)
            return validationResponse;



        if (!TryDecryptLong(request.clubId, out var decryptedClubId) || decryptedClubId <= 0)
        {
            return await ValidationErrorResponseAsync(ResponseCodes.CodeCUE101_Invalid_club);
        }

        if (!TryDecryptLong(request.planId, out var decryptedPlanId) || decryptedPlanId <= 0)
        {
            return await ValidationErrorResponseAsync(ResponseCodes.CUERES132_PlanIdRequired);
        }

        if (request.noOfPeople <= 0)
        {
            return await ValidationErrorResponseAsync(ResponseCodes.CUERES129_NumberOfPeopleRequired);
        }

        string? decryptedHostIds = null;

        if (!string.IsNullOrWhiteSpace(request.hostIds))
        {
            try
            {
                decryptedHostIds = string.Join(
                    ",",
                    request.hostIds
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(hostId => _stringCipher.Decrypt(hostId))
                        .Where(hostId => !string.IsNullOrWhiteSpace(hostId))
                );
            }
            catch
            {
                return await BadRequestResponseAsync(ResponseCodes.Code400_BadRequest);
            }
        }

        if (request.visitDateTimeMS <= 0)
        {
            return (
               HttpStatusCode.BadRequest,
               CommonApiResponseBuilder.Create()
                   .WithCode(ResponseCodes.Code400_BadRequest)
                   .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest))
                   .WithErrors(new List<Errors>
                   {
                            new Errors
                                {
                                    errorCode = ResponseCodes.CUERES131_ReservationDateRequired,
                                    errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUERES131_ReservationDateRequired)
                                },
                        })
                        .Build()
                    );
        }

        var decryptedCustomerPreferencesJson =
            DecryptCustomerPreferencesOrEmpty(request.customerPreferencesJson);

        var requestContext = GetRequestContext();

        var mobileNumber = Normalize(request.mobileNumber);

        if (string.IsNullOrWhiteSpace(mobileNumber))
        {
            return await BadRequestResponseAsync(ResponseCodes.Code400_BadRequest);
        }

        // var subDomainName = Helper.GetSubdomain(_requestSourceUrl, _baseDomains);
        // bool isViaMultitenant = string.IsNullOrEmpty(subDomainName) ? false : true;
        var subDomainName =
           _httpContextAccessor?.HttpContext?.Items["RequestSubdomain"] as string;
        bool isViaMultitenant = string.IsNullOrEmpty(subDomainName) ? false : true;

        var dbRequest = new ReservationHoldConfirmRequestCommon
        {
            clubId = decryptedClubId,
            planId = decryptedPlanId,
            noOfPeople = request.noOfPeople,
            hostIds = decryptedHostIds,
            visitDateTimeMS = request.visitDateTimeMS,
            timeZoneOffsetValue = requestContext.TimeZoneOffsetValue,
            actionIP = requestContext.ActionIP,
            actionPlatform = requestContext.ActionPlatform,
            actionUserId = mobileNumber,
            customerPreferencesJson = decryptedCustomerPreferencesJson,
            mobileNumber = mobileNumber,
            isViaMultitenant = isViaMultitenant,
            referCode = request.referCode,
            referType = request.referType
        };

        var reservationValidationRequest = new ReservationValidateRequestCommon
        {
            clubId = dbRequest.clubId,
            planId = dbRequest.planId,
            selectedDateTime = dbRequest.visitDateTimeMS,
            timeZoneOffsetValue = dbRequest.timeZoneOffsetValue
        };

        var reservationValidationResponse =
            await _repos.IsReservationValid(reservationValidationRequest);

        if (!IsDbSuccess(reservationValidationResponse?.code))
        {
            await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
                eventName: "invalid_reservation_request",
                stepCode: "otp",
                eventStatus: "failure",
                relatedEntityType: "reservation_hold",
                errorCategory: "reservation_hold",
                errorCode: reservationValidationResponse?.code ?? "",
                errorMessage: reservationValidationResponse?.message ?? "",
                properties: new
                {
                    clubId = decryptedClubId,
                    planId = decryptedPlanId
                });
            return await BadRequestResponseAsync(
                ResolveMessageCode(reservationValidationResponse?.message)
            );

        }

        _sqlTransactionHandler.BeginTransaction();
        try
        {
            var dbResponse = await _repos.ConfirmReservationHold(dbRequest);

            if (!IsDbSuccess(dbResponse?.code))
            {

                _sqlTransactionHandler.RollbackTransaction();
                await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
                    eventName: "invalid_reservation_request",
                    stepCode: "otp",
                    eventStatus: "failure",
                    relatedEntityType: "reservation_hold",
                    errorCategory: "reservation_hold",
                    errorCode: dbResponse?.code ?? "",
                    errorMessage: dbResponse?.message ?? "",
                    properties: new
                    {
                        errorCode = dbResponse?.message,
                        clubId = decryptedClubId,
                        planId = decryptedPlanId
                    });
                return await BadRequestResponseAsync(
                    ResolveMessageCode(dbResponse?.message)
                );
            }

            string purpose = dbResponse.doesCustomerExists == false
                ? OtpPurposeConstants.Register
                : OtpPurposeConstants.Login;

            if (!dbResponse.canReserve)
            {
                _sqlTransactionHandler.RollbackTransaction();

                var canReserveMessageCode = ResolveMessageCode(
                    string.IsNullOrWhiteSpace(dbResponse.message)
                        ? ResponseCodes.Code200_Success
                        : dbResponse.message
                );
                var canReserveResponseData = new ReservationHoldConfirmResponseModel
                {
                    canReserve = false,
                    nextEligibleDate = dbResponse.nextEligibleDate,
                    previousVisitDate = dbResponse.previousVisitDate
                };
                return (
                    HttpStatusCode.OK,
                    CommonApiResponseBuilder.Create()
                        .WithCode(ResponseCodes.Code200_Success)
                        .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(canReserveMessageCode))
                        .WithData(canReserveResponseData)
                        .Build()
                );
            }

            var username = dbResponse.doesCustomerExists == false
                ? mobileNumber
                : Normalize(dbResponse.customerCognitoUsername);

            bool isCognitoUserName = dbResponse.doesCustomerExists == false
                ? false
                : true;

            var otpCode = Normalize(dbResponse.otpCode);
            var processId = Normalize(dbResponse.processId);

            if
            (
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(otpCode) ||
                string.IsNullOrWhiteSpace(processId)
            )
            {
                _sqlTransactionHandler.RollbackTransaction();
                return await BadRequestResponseAsync(ResponseCodes.CodeCUE125_Something_Went_Wrong_Contact_Admin);
            }

            var cognitoAuthSession = string.Empty;

            if (IsLoginPurpose(purpose))
            {
                var customAuthResponse = await _cognitoAuthService.StartCustomAuthAsync(
                    username,
                    isCognitoUserName,
                    requestContext.CancellationToken
                );

                if (!IsDbSuccess(customAuthResponse?.code) ||
                    string.IsNullOrWhiteSpace(customAuthResponse?.session))
                {
                    _sqlTransactionHandler.RollbackTransaction();
                    return await BadRequestResponseAsync(
                        ResolveMessageCode(customAuthResponse?.message)
                    );
                }

                cognitoAuthSession = customAuthResponse.session;
            }

            var otpHash = OtpHashHelper.HashOtp(
                username,
                purpose,
                otpCode,
                _cognitoOptions.OtpHashSecret
            );

            var otpSendRequest = new ReservationSendOtpRequestCommon
            {
                mobileNumber = mobileNumber,
                otpCode = otpCode,
                otpHash = otpHash,
                reservationHoldId = dbResponse.reservationHoldId,
                actionIP = dbRequest.actionIP,
                actionPlatform = dbRequest.actionPlatform,
                traceId = processId,
                cognitoAuthSession = cognitoAuthSession,
                username = username
            };

            var otpSendResponse = await _repos.ReservationSendOtp(otpSendRequest);

            if (!IsDbSuccess(otpSendResponse?.code))
            {
                _sqlTransactionHandler.RollbackTransaction();
                return await BadRequestResponseAsync(
                    ResolveMessageCode(otpSendResponse?.message)
                );
            }

            await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
                eventName: "otp_sent",
                stepCode: "otp",
                eventStatus: "success",
                relatedEntityType: "reservation_hold",
                relatedEntityId: dbResponse.reservationHoldId.ToString(),
                properties: new
                {
                    reservationHoldId = dbResponse.reservationHoldId,
                    doesCustomerExists = dbResponse.doesCustomerExists
                });

            _sqlTransactionHandler.CommitTransaction();

            var responseData = new ReservationHoldConfirmResponseModel
            {
                canReserve = true,
                reservationHoldId = _stringCipher.Encrypt(dbResponse.reservationHoldId.ToString()),
                isViaMultitenant = dbResponse.isViaMultitenant,
                hasMultitenantAffiliate = dbResponse.hasMultitenantAffiliate,
                doesCustomerExists = dbResponse.doesCustomerExists,
                processId = processId,
                expiryTime = Convert.ToInt32(_configuration["UserReservationHoldConfirmationCodeExpiryTimeInMilliseconds"])
            };

            var successMessageCode = ResolveMessageCode(
                string.IsNullOrWhiteSpace(dbResponse.message)
                    ? ResponseCodes.Code200_Success
                    : dbResponse.message
            );

            return (
                HttpStatusCode.OK,
                CommonApiResponseBuilder.Create()
                    .WithCode(ResponseCodes.Code200_Success)
                    .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(successMessageCode))
                    .WithData(responseData)
                    .Build()
            );
        }
        catch (Exception ex)
        {
            _sqlTransactionHandler.RollbackTransaction();
            throw new Exception("Error occurred.", ex);
        }
    }

    public async Task<(HttpStatusCode, object)> ReservationHoldResendOtp(ReservationHoldReSendOtpRequestModel request, ConversionSessionContext session)
    {
        var validationResponse = await _validationHelper.ValidateModelAndSignatureAsync(
            request,
            x => new ReservationHoldReSendOtpRequestModelValidatorModel().Validate(x)
        );

        if (validationResponse.Item1 != HttpStatusCode.OK)
            return validationResponse;



        if (!TryDecryptLong(request.reservationHoldId, out var decryptedReservationHoldId) ||
                  decryptedReservationHoldId <= 0)
        {
            return await ValidationErrorResponseAsync(ResponseCodes.CUERES138_ReservationHoldIdRequired);
        }

        var holdDetailResponse =
            await _repos.ReservationHoldDetails(decryptedReservationHoldId);

        if (!IsDbSuccess(holdDetailResponse?.code))
        {
            await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
                eventName: "invalid_otp_resend_request",
                stepCode: "otp",
                eventStatus: "failure",
                relatedEntityType: "reservation_hold",
                relatedEntityId: decryptedReservationHoldId.ToString(),
                errorCategory: "otp_resend",
                errorCode: holdDetailResponse?.code ?? "",
                errorMessage: holdDetailResponse?.message ?? "",
                properties: new
                {
                    reservationHoldId = decryptedReservationHoldId
                });
            return await BadRequestResponseAsync(
                ResolveMessageCode(holdDetailResponse?.message)
            );
        }

        var requestContext = GetRequestContext();

        var mobileNumber = Normalize(request.mobileNumber);

        if (string.IsNullOrWhiteSpace(mobileNumber))
        {
            return await BadRequestResponseAsync(ResponseCodes.Code400_BadRequest);
        }

        string purpose = holdDetailResponse.customerId > 0
            && !string.IsNullOrEmpty(holdDetailResponse.customAuthUsername)
            ? OtpPurposeConstants.Login
            : OtpPurposeConstants.Register;

        var username = holdDetailResponse.customerId > 0
            && !string.IsNullOrEmpty(holdDetailResponse.customAuthUsername)
            ? Normalize(holdDetailResponse.customAuthUsername)
            : mobileNumber;

        bool isCognitoUserName = !string.IsNullOrEmpty(holdDetailResponse.customAuthUsername)
            ? true
            : false;

        var otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var processId = Normalize(holdDetailResponse.processId);

        if
        (
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(otpCode) ||
            string.IsNullOrWhiteSpace(processId)
        )
        {
            return await BadRequestResponseAsync(ResponseCodes.CodeCUE125_Something_Went_Wrong_Contact_Admin);
        }

        var cognitoAuthSession = string.Empty;

        if (IsLoginPurpose(purpose))
        {
            var customAuthResponse = await _cognitoAuthService.StartCustomAuthAsync(
                username,
                isCognitoUserName,
                requestContext.CancellationToken
            );

            if (!IsDbSuccess(customAuthResponse?.code) ||
                string.IsNullOrWhiteSpace(customAuthResponse?.session))
            {
                return await BadRequestResponseAsync(
                    ResolveMessageCode(customAuthResponse?.message)
                );
            }

            cognitoAuthSession = customAuthResponse.session;
        }

        var otpHash = OtpHashHelper.HashOtp(
            username,
            purpose,
            otpCode,
            _cognitoOptions.OtpHashSecret
        );
        var otpSendRequest = new ReservationReSendOtpRequestCommon
        {
            mobileNumber = mobileNumber,
            otpCode = otpCode,
            otpHash = otpHash,
            reservationHoldId = holdDetailResponse.reservationHoldId,
            actionIP = requestContext.ActionIP,
            actionPlatform = requestContext.ActionPlatform,
            cognitoAuthSession = cognitoAuthSession
        };

        var otpSendResponse = await _repos.ReservationReSendOtp(otpSendRequest);

        if (!IsDbSuccess(otpSendResponse?.code))
        {
            await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
                eventName: "invalid_otp_resend_request",
                stepCode: "otp",
                eventStatus: "failure",
                relatedEntityType: "reservation_hold",
                relatedEntityId: decryptedReservationHoldId.ToString(),
                errorCategory: "otp_resend",
                errorCode: otpSendResponse?.code ?? "",
                errorMessage: otpSendResponse?.message ?? "",
                properties: new
                {
                    reservationHoldId = decryptedReservationHoldId
                });
            return await BadRequestResponseAsync(
                ResolveMessageCode(otpSendResponse?.message)
            );
        }

        await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
            eventName: "otp_resent",
            stepCode: "otp",
            eventStatus: "success",
            relatedEntityType: "reservation_hold",
            relatedEntityId: decryptedReservationHoldId.ToString(),
            properties: new
            {
                reservationHoldId = decryptedReservationHoldId
            });
        var subDomainName = Helper.GetSubdomain(_requestSourceUrl, _baseDomains);
        bool isViaMultitenant = string.IsNullOrEmpty(subDomainName) ? false : true;

        var responseData = new ReservationHoldConfirmResponseModel
        {
            reservationHoldId = _stringCipher.Encrypt(holdDetailResponse.reservationHoldId.ToString()),
            isViaMultitenant = isViaMultitenant,
            hasMultitenantAffiliate = holdDetailResponse.hasMultitenantAffiliate,
            doesCustomerExists = holdDetailResponse.doesCustomerExists,
            processId = processId,
            expiryTime = Convert.ToInt32(_configuration["UserReservationHoldConfirmationCodeExpiryTimeInMilliseconds"])
        };

        var successMessageCode = ResolveMessageCode(
            string.IsNullOrWhiteSpace(otpSendResponse.message)
                ? ResponseCodes.Code200_Success
                : otpSendResponse.message
        );

        return (
            HttpStatusCode.OK,
            CommonApiResponseBuilder.Create()
                .WithCode(ResponseCodes.Code200_Success)
                .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(successMessageCode))
                .WithData(responseData)
                .Build()
        );
    }

    public async Task<(HttpStatusCode, object)> ReservationVerifyOtp(ReservationVerifyOtpRequestModel request, ConversionSessionContext session)
    {
        var validationResponse = await _validationHelper.ValidateModelAndSignatureAsync(
            request,
            x => new ReservationVerifyOtpRequestValidatorModel().Validate(x)
        );

        if (validationResponse.Item1 != HttpStatusCode.OK)
            return validationResponse;



        if (!TryDecryptLong(request.reservationHoldId, out var decryptedReservationHoldId) ||
            decryptedReservationHoldId <= 0)
        {
            return await ValidationErrorResponseAsync(ResponseCodes.CUERES138_ReservationHoldIdRequired);
        }

        var holdDetailResponse =
            await _repos.ReservationHoldDetails(decryptedReservationHoldId);

        if (!IsDbSuccess(holdDetailResponse?.code))
        {
            return await BadRequestResponseAsync(
                ResolveMessageCode(holdDetailResponse?.message)
            );
        }

        string encryptedHostIds = string.Join(
                            ",",
                            holdDetailResponse.hostId
                                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                .Select(hostId => _stringCipher.Encrypt(hostId))
                                .Where(hostId => !string.IsNullOrWhiteSpace(hostId))
                        );
        var encryptedCustomerPreferencesJson =
           EncryptCustomerPreferencesOrEmpty(holdDetailResponse.customerPreferencesJson);

        var requestContext = GetRequestContext();

        var customAuthPurpose = Normalize(holdDetailResponse.customAuthPurpose);
        var customAuthUsername = Normalize(holdDetailResponse.customAuthUsername);
        var cognitoAuthSession = Normalize(holdDetailResponse.cognitoAuthSession);

        if (string.IsNullOrWhiteSpace(customAuthPurpose)
            || string.IsNullOrWhiteSpace(customAuthUsername))
            return await BadRequestResponseAsync(ResponseCodes.CodeCUE125_Something_Went_Wrong_Contact_Admin);

        ReservationDetailsModel BuildReservationData()
        {
            return new ReservationDetailsModel
            {
                clubId = _stringCipher.Encrypt(holdDetailResponse.clubId.ToString()),
                noOfPeople = holdDetailResponse.noOfPeople,
                hostIds = encryptedHostIds,
                planId = _stringCipher.Encrypt(holdDetailResponse.planId.ToString()),
                visitDateTimeMS = holdDetailResponse.visitDateTimeMS,
                customerPreferencesJson = encryptedCustomerPreferencesJson
            };
        }

        if (IsLoginPurpose(customAuthPurpose))
        {
            if (string.IsNullOrWhiteSpace(cognitoAuthSession))
                return await BadRequestResponseAsync(ResponseCodes.CodeCUE125_Something_Went_Wrong_Contact_Admin);

            var cognitoResponse =
                await _cognitoAuthService.ResponseToCustomAuthChallengeAync(
                    customAuthUsername,
                    cognitoAuthSession,
                    request.otpCode,
                    requestContext.CancellationToken
                );

            if (!IsDbSuccess(cognitoResponse?.code))
            {
                var commonErrorMessage = ResolveMessageCode(
                    ResponseCodes.CodeCUE125_Something_Went_Wrong_Contact_Admin
                );

                await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
                    eventName: "otp_verification_failed",
                    stepCode: "otp",
                    eventStatus: "failure",
                    relatedEntityType: "reservation_hold",
                    relatedEntityId: decryptedReservationHoldId.ToString(),
                    errorCategory: "otp_verification",
                    errorCode: cognitoResponse?.code ?? "",
                    errorMessage: cognitoResponse?.message ?? "",
                    properties: new
                    {
                        reservationHoldId = decryptedReservationHoldId
                    });

                if (string.IsNullOrWhiteSpace(cognitoResponse?.latestSession))
                {
                    return await BadRequestResponseAsync(commonErrorMessage);
                }

                var updateSessionResponse = await _repos.ReservationUpdateCognitoSession(
                    new ReservationUpdateCognitoSessionRequestCommon
                    {
                        reservationHoldId = decryptedReservationHoldId,
                        processId = holdDetailResponse.processId,
                        cognitoAuthSession = cognitoResponse.latestSession
                    });

                if (!IsDbSuccess(updateSessionResponse?.code))
                {
                    return await BadRequestResponseAsync(commonErrorMessage);
                }

                var responseMessageCode = !string.IsNullOrWhiteSpace(cognitoResponse?.message)
                    ? cognitoResponse.message
                    : ResponseCodes.CodeCUE125_Something_Went_Wrong_Contact_Admin;

                return await BadRequestResponseAsync(
                    ResolveMessageCode(responseMessageCode)
                );
            }

            await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
                eventName: "otp_verified",
                stepCode: "otp",
                eventStatus: "success",
                relatedEntityType: "reservation_hold",
                relatedEntityId: decryptedReservationHoldId.ToString(),
                properties: new
                {
                    reservationHoldId = decryptedReservationHoldId
                });

            await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
                eventName: "authentication_completed",
                stepCode: "identity",
                eventStatus: "success",
                relatedEntityType: "customer",
                properties: new
                {
                    reservationHoldId = decryptedReservationHoldId
                });

            await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
                eventName: "identity_resolved",
                stepCode: "identity",
                eventStatus: "success",
                relatedEntityType: "customer",
                relatedEntityId: holdDetailResponse.customerId.ToString(),
                properties: new
                {
                    reservationHoldId = decryptedReservationHoldId,
                    customerExists = true
                });

            var tokenPair = new TokenPairModel
            {
                scheme = cognitoResponse?.tokenType,
                accessToken = cognitoResponse?.accessToken,
                refreshToken = cognitoResponse?.refreshToken,
                accessTokenExpiry = Convert.ToInt64(_configuration["AccessTokenExpiryTimeInMilliseconds"]),
                refreshTokenExpiry = Convert.ToInt64(_configuration["RefreshTokenExpiryInMilliseconds"]),
                isPasswordForceful = false
            };

            var manageTokenDBRequest = new ManageTokenRequestCommon
            {
                actionUserId = holdDetailResponse?.userId,
                roleType = holdDetailResponse?.roleType,
                accessToken = cognitoResponse?.accessToken,
                accessTokenExpiryTimeMS = tokenPair.accessTokenExpiry,
                refreshToken = tokenPair.refreshToken,
                refreshTokenExpiryTimeMS = tokenPair.refreshTokenExpiry
            };

            var manageTokenDBResponse = await _authRepository.ManageToken(manageTokenDBRequest);
            if (manageTokenDBResponse?.code != "0")
                return await BadRequestResponseAsync(
                    ResolveMessageCode(manageTokenDBResponse?.message)
                );

            var responseData = new ReservationVerifyOtpResponseModel
            {
                customerExists = true,
                reservationHoldId = request.reservationHoldId,
                tokenPair = tokenPair,
                reservationData = BuildReservationData()
            };

            return await SuccessResponseAsync(responseData);
        }

        if (IsRegisterPurpose(customAuthPurpose))
        {
            var submittedOtpHash = OtpHashHelper.HashOtp(
                request.mobileNumber,
                OtpPurposeConstants.Register,
                request.otpCode,
                _cognitoOptions.OtpHashSecret
            );

            var subDomainName = Helper.GetSubdomain(_requestSourceUrl, _baseDomains);
            bool isViaMultitenant = string.IsNullOrEmpty(subDomainName) ? false : true;

            var verifyOtpRequest = new ReservationVerifyOtpRequestCommon
            {
                mobileNumber = request.mobileNumber,
                reservationHoldId = decryptedReservationHoldId,
                cognitoPurpose = OtpPurposeConstants.Register,
                submittedOtpHash = submittedOtpHash,
                actionIP = requestContext.ActionIP,
                actionPlatform = requestContext.ActionPlatform,
                isViaMultitenant = isViaMultitenant
            };

            var verifyOtpResponse =
                await _repos.ReservationVerifyOtp(verifyOtpRequest);

            if (!IsDbSuccess(verifyOtpResponse?.code))
            {
                await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
                    eventName: "otp_verification_failed",
                    stepCode: "otp",
                    eventStatus: "failure",
                    relatedEntityType: "reservation_hold",
                    relatedEntityId: decryptedReservationHoldId.ToString(),
                    errorCategory: "otp_verification",
                    errorCode: verifyOtpResponse?.code ?? "",
                    errorMessage: verifyOtpResponse?.message ?? "",
                    properties: new
                    {
                        errorCode = verifyOtpResponse?.message,
                        reservationHoldId = decryptedReservationHoldId
                    });
                return await BadRequestResponseAsync(
                    ResolveMessageCode(verifyOtpResponse?.message)
                );
            }

            await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
                eventName: "otp_verified",
                stepCode: "otp",
                eventStatus: "success",
                relatedEntityType: "reservation_hold",
                relatedEntityId: decryptedReservationHoldId.ToString(),
                properties: new
                {
                    reservationHoldId = decryptedReservationHoldId
                });

            await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
                eventName: "registration_started",
                stepCode: "identity",
                eventStatus: "success",
                relatedEntityType: "customer",
                properties: new
                {
                    reservationHoldId = decryptedReservationHoldId,
                    customerExists = false
                });

            var responseData = new ReservationVerifyOtpResponseModel
            {
                customerExists = false,
                reservationHoldId = request.reservationHoldId,
                tokenPair = null,
                reservationData = BuildReservationData()
            };

            return await SuccessResponseAsync(responseData);
        }

        return await BadRequestResponseAsync(ResponseCodes.CodeCUE125_Something_Went_Wrong_Contact_Admin);
    }

    #endregion

    #region Reservation Customer
    public async Task<(HttpStatusCode, object)> ReservationRegisterCustomer(
        ReservationRegisterCustomerRequestModel request, ConversionSessionContext session)
    {
        var validationResponse = await _validationHelper.ValidateModelAndSignatureAsync(
            request,
            x => new ReservationRegisterCustomerRequestValidatorModel().Validate(x));

        if (validationResponse.Item1 != HttpStatusCode.OK)
            return validationResponse;



        if (!TryDecryptLong(request.reservationHoldId, out var decryptedReservationHoldId) ||
            decryptedReservationHoldId <= 0)
        {
            return await ValidationErrorResponseAsync(
                ResponseCodes.CUERES138_ReservationHoldIdRequired);
        }

        if (!IsValidDob(request.dob))
            return await ValidationErrorResponseAsync(ResponseCodes.CUE120_InvalidDateFormat);

        request.dob = NormalizeDob(request.dob);

        var requestContext = GetRequestContext();

        var holdDetailResponse =
            await _repos.ReservationHoldConfirmedDetails(decryptedReservationHoldId);

        if (!IsDbSuccess(holdDetailResponse?.code))
        {
            return await BadRequestResponseAsync(
                ResolveMessageCode(holdDetailResponse?.message));
        }

        var encryptedHostIds = EncryptCommaSeparatedIds(holdDetailResponse?.hostId);

        var encryptedCustomerPreferencesJson =
            EncryptCustomerPreferencesOrEmpty(holdDetailResponse?.customerPreferencesJson);

        var subDomainName = Helper.GetSubdomain(_requestSourceUrl, _baseDomains);
        var isViaMultitenant = !string.IsNullOrWhiteSpace(subDomainName);

        var registrationDBRequest = new ReservationRegisterCustomerRequestCommon
        {
            nickName = request.nickName,
            emailAddress = request.emailAddress,
            mobileNumber = request.mobileNumber,
            dob = request.dob,
            reservationHoldId = decryptedReservationHoldId,
            actionIP = requestContext.ActionIP,
            actionPlatform = requestContext.ActionPlatform,
            actionUserId = request.mobileNumber,
            isViaMultitenant = isViaMultitenant
        };

        var transactionCompleted = false;
        string? cognitoUsername = null;
        string? customerAgentId = null;

        try
        {
            _sqlTransactionHandler.BeginTransaction();

            var registrationDBResponse =
                await _repos.ReservationRegisterCustomer(registrationDBRequest);

            if (!IsDbSuccess(registrationDBResponse?.code))
            {
                SafeRollbackTransaction();
                transactionCompleted = true;

                await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
                    eventName: "registration_failed",
                    stepCode: "identity",
                    eventStatus: "failure",
                    relatedEntityType: "customer",
                    errorCategory: "registration",
                    errorCode: registrationDBResponse?.code ?? "",
                    errorMessage: registrationDBResponse?.message ?? "",
                    properties: new
                    {
                        reservationHoldId = decryptedReservationHoldId
                    });

                return await BadRequestResponseAsync(
                    ResolveMessageCode(registrationDBResponse?.message));
            }

            cognitoUsername = registrationDBResponse?.cognitoUsername?.Trim().ToLowerInvariant();
            customerAgentId = registrationDBResponse?.agentId;

            if (string.IsNullOrWhiteSpace(cognitoUsername))
            {
                SafeRollbackTransaction();
                transactionCompleted = true;

                await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
                    eventName: "registration_failed",
                    stepCode: "identity",
                    eventStatus: "failure",
                    relatedEntityType: "customer",
                    errorCategory: "registration",
                    errorCode: ResponseCodes.CUEREG101_DuplicateMobileNumberOrNickname,
                    errorMessage: await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUEREG101_DuplicateMobileNumberOrNickname),
                    properties: new
                    {
                        reservationHoldId = decryptedReservationHoldId
                    });


                return await BadRequestResponseAsync(
                    ResponseCodes.CUEREG101_DuplicateMobileNumberOrNickname);
            }

            var countryCode = _configuration["CountryCode"];
            var formattedMobileNumber = FormatPhoneNumber(request.mobileNumber, countryCode);

            var duplicateValidationCode =
                await RemoveUnconfirmedUsersOrGetDuplicateCodeAsync(
                    formattedMobileNumber,
                    cognitoUsername,
                    requestContext.ActionIP,
                    "CustomerAPI");

            if (!string.IsNullOrWhiteSpace(duplicateValidationCode))
            {
                SafeRollbackTransaction();
                transactionCompleted = true;

                await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
                    eventName: "registration_failed",
                    stepCode: "identity",
                    eventStatus: "failure",
                    relatedEntityType: "customer",
                    errorCategory: "registration",
                    errorCode: duplicateValidationCode,
                    properties: new
                    {
                        reservationHoldId = decryptedReservationHoldId
                    });

                return await BadRequestResponseAsync(duplicateValidationCode);
            }

            var adminCreateUser = await _amazonCognitoMiddleware.AdminCreateUserAsync(
                new common.Models.Middleware.AmazonCognitoModel.SignUp.SignUpModel.Request
                {
                    Username = cognitoUsername,
                    Password = registrationDBResponse?.password,
                    AttributeType =
                    [
                        new common.Models.Middleware.AmazonCognitoModel.SignUp.SignUpModel.AttributeType
                            {
                                Name = "email",
                                Value = request.emailAddress
                            },
                            new common.Models.Middleware.AmazonCognitoModel.SignUp.SignUpModel.AttributeType
                            {
                                Name = "phone_number",
                                Value = formattedMobileNumber
                            }
                    ]
                });

            if (adminCreateUser?.Code != ResponseCode.Success)
            {
                SafeRollbackTransaction();
                transactionCompleted = true;

                await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
                    eventName: "registration_failed",
                    stepCode: "identity",
                    eventStatus: "failure",
                    relatedEntityType: "customer",
                    errorCategory: "registration",
                    errorCode: adminCreateUser?.Code.ToString() ?? "",
                    errorMessage: adminCreateUser?.Errors?.FirstOrDefault()?.ErrorMessage ?? "",
                    properties: new
                    {
                        reservationHoldId = decryptedReservationHoldId
                    });

                return await BadRequestResponseAsync(
                    adminCreateUser?.Errors?.FirstOrDefault()?.ErrorMessage);
            }

            var markMobileNumberAsVerifiedResponse =
                await _amazonCognitoMiddleware.MarkMobileNumberAsVerifiedAsync(
                    new MarkMobileNumberAsVerifiedModel.Request
                    {
                        Username = cognitoUsername,
                        MobileNumber = formattedMobileNumber
                    });

            if (markMobileNumberAsVerifiedResponse?.Code != ResponseCode.Success)
            {
                SafeRollbackTransaction();
                transactionCompleted = true;

                await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
                    eventName: "registration_failed",
                    stepCode: "identity",
                    eventStatus: "failure",
                    relatedEntityType: "customer",
                    errorCategory: "registration",
                    errorCode: markMobileNumberAsVerifiedResponse?.Errors?.FirstOrDefault()?.ErrorMessage ?? "",
                    properties: new
                    {
                        reservationHoldId = decryptedReservationHoldId
                    });

                return await BadRequestResponseAsync(
                    markMobileNumberAsVerifiedResponse?.Errors?.FirstOrDefault()?.ErrorMessage);
            }

            var adminCreateUserObject =
                adminCreateUser.Data.MapObjects<
                    common.Models.Middleware.AmazonCognitoModel.SignUp.SignUpModel.AdminCreateUserResponse>();

            var userSub = adminCreateUserObject
                .FirstOrDefault(x => string.Equals(x.Name, "sub", StringComparison.OrdinalIgnoreCase))
                ?.Value;

            if (string.IsNullOrWhiteSpace(userSub))
            {
                SafeRollbackTransaction();
                transactionCompleted = true;

                await HandleRegistrationFailureAsync(cognitoUsername, customerAgentId);


                await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
                    eventName: "registration_failed",
                    stepCode: "identity",
                    eventStatus: "failure",
                    relatedEntityType: "customer",
                    errorCategory: "registration",
                    errorCode: adminCreateUser?.Errors?.FirstOrDefault()?.ErrorMessage
                              ?? ResponseCodes.CUEREG101_DuplicateMobileNumberOrNickname,
                    errorMessage: await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUEREG101_DuplicateMobileNumberOrNickname),
                    properties: new
                    {
                        reservationHoldId = decryptedReservationHoldId
                    });

                return await BadRequestResponseAsync(
                    adminCreateUser?.Errors?.FirstOrDefault()?.ErrorMessage
                    ?? ResponseCodes.CUEREG101_DuplicateMobileNumberOrNickname);
            }

            _sqlTransactionHandler.CommitTransaction();
            transactionCompleted = true;

            var updateCustomerDetailResponse =
                await _repos.ReservationUpdateCustomerDetail(
                    new ReservationCustomerDetailsUpdateRequestCommon
                    {
                        agentId = customerAgentId,
                        cognitoUserId = userSub
                    });

            if (!IsDbSuccess(updateCustomerDetailResponse?.code))
            {
                await HandleRegistrationFailureAsync(cognitoUsername, customerAgentId);

                await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
                    eventName: "registration_failed",
                    stepCode: "identity",
                    eventStatus: "failure",
                    relatedEntityType: "customer",
                    errorCode: updateCustomerDetailResponse?.code ?? "",
                    errorMessage: updateCustomerDetailResponse?.message ?? "",
                    errorCategory: "registration",
                    properties: new
                    {
                        reservationHoldId = decryptedReservationHoldId
                    });

                return await BadRequestResponseAsync(
                    ResolveMessageCode(updateCustomerDetailResponse?.message));
            }

            var customAuthResponse = await _cognitoAuthService.StartCustomAuthAsync(
                cognitoUsername,
                true,
                requestContext.CancellationToken);

            if (!IsDbSuccess(customAuthResponse?.code) ||
                string.IsNullOrWhiteSpace(customAuthResponse?.session))
            {
                await HandleRegistrationFailureAsync(cognitoUsername, customerAgentId);

                await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
                    eventName: "registration_failed",
                    stepCode: "identity",
                    eventStatus: "failure",
                    relatedEntityType: "customer",
                    errorCode: customAuthResponse?.code ?? "",
                    errorMessage: customAuthResponse?.message ?? "",
                    errorCategory: "registration",
                    properties: new
                    {
                        reservationHoldId = decryptedReservationHoldId
                    });

                return await BadRequestResponseAsync(
                    ResolveMessageCode(customAuthResponse?.message));
            }

            var cognitoAuthSession = customAuthResponse.session;
            var purpose = OtpPurposeConstants.Login;
            var otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

            var otpHash = OtpHashHelper.HashOtp(
                cognitoUsername,
                purpose,
                otpCode,
                _cognitoOptions.OtpHashSecret);

            var otpSendResponse = await _authRepository.CreateOtpAsync(
                cognitoUsername,
                purpose,
                otpHash,
                otpCode,
                holdDetailResponse.processId,
                cognitoAuthSession);

            if (!IsDbSuccess(otpSendResponse?.code))
            {
                await HandleRegistrationFailureAsync(cognitoUsername, customerAgentId);

                await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
                    eventName: "registration_failed",
                    stepCode: "identity",
                    eventStatus: "failure",
                    relatedEntityType: "customer",
                    errorCode: otpSendResponse?.code ?? "",
                    errorMessage: otpSendResponse?.message ?? "",
                    errorCategory: "registration",
                    properties: new
                    {
                        reservationHoldId = decryptedReservationHoldId
                    });

                return await BadRequestResponseAsync(
                    ResolveMessageCode(otpSendResponse?.message));
            }

            var cognitoResponse =
                await _cognitoAuthService.ResponseToCustomAuthChallengeAync(
                    cognitoUsername,
                    cognitoAuthSession,
                    otpCode,
                    requestContext.CancellationToken);

            if (!IsDbSuccess(cognitoResponse?.code))
            {
                await HandleRegistrationFailureAsync(cognitoUsername, customerAgentId);

                await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
                    eventName: "registration_failed",
                    stepCode: "identity",
                    eventStatus: "failure",
                    relatedEntityType: "customer",
                    errorCode: cognitoResponse?.code ?? "",
                    errorMessage: cognitoResponse?.message ?? "",
                    errorCategory: "registration",
                    properties: new
                    {
                        reservationHoldId = decryptedReservationHoldId
                    });

                return await BadRequestResponseAsync(
                    ResolveMessageCode(cognitoResponse?.message));
            }

            await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
                eventName: "identity_resolved",
                stepCode: "identity",
                eventStatus: "success",
                relatedEntityType: "customer",
                relatedEntityId: registrationDBResponse?.agentId ?? "",
                properties: new
                {
                    reservationHoldId = decryptedReservationHoldId,
                    customerExists = true
                });

            var tokenPair = BuildTokenPair(cognitoResponse);

            var manageTokenDBResponse = await _authRepository.ManageToken(
                new ManageTokenRequestCommon
                {
                    actionUserId = registrationDBResponse?.userId,
                    roleType = registrationDBResponse?.roleType,
                    accessToken = cognitoResponse?.accessToken,
                    accessTokenExpiryTimeMS = tokenPair.accessTokenExpiry,
                    refreshToken = tokenPair.refreshToken,
                    refreshTokenExpiryTimeMS = tokenPair.refreshTokenExpiry
                });

            if (!IsDbSuccess(manageTokenDBResponse?.code))
            {
                await HandleRegistrationFailureAsync(cognitoUsername, customerAgentId);

                return await BadRequestResponseAsync(
                    ResolveMessageCode(manageTokenDBResponse?.message));
            }

            var responseData = new ReservationVerifyOtpResponseModel
            {
                customerExists = true,
                reservationHoldId = request.reservationHoldId,
                tokenPair = tokenPair,
                reservationData = new ReservationDetailsModel
                {
                    clubId = _stringCipher.Encrypt(holdDetailResponse.clubId.ToString()),
                    noOfPeople = holdDetailResponse.noOfPeople,
                    hostIds = encryptedHostIds,
                    planId = _stringCipher.Encrypt(holdDetailResponse.planId.ToString()),
                    visitDateTimeMS = holdDetailResponse.visitDateTimeMS,
                    customerPreferencesJson = encryptedCustomerPreferencesJson
                }
            };

            return await SuccessResponseAsync(responseData);
        }
        catch (Exception ex)
        {
            if (!transactionCompleted)
                SafeRollbackTransaction();

            await HandleRegistrationFailureAsync(cognitoUsername, customerAgentId);

            await TrackConversionEvent(session, journeyGuid: GetJourneyGuid(),
                eventName: "registration_failed",
                stepCode: "registration",
                eventStatus: "failure",
                relatedEntityType: "customer",
                errorMessage: ex.Message,
                properties: new
                {
                    reservationHoldId = decryptedReservationHoldId
                });

            throw new Exception("Error occurred.", ex);
        }
    }

    private static bool IsValidDob(string? dob)
    {
        if (string.IsNullOrWhiteSpace(dob))
            return true;

        var trimmedDob = dob.Trim();

        if (trimmedDob == "--")
            return true;

        const string pattern =
            @"^(?:\d{4}[-/]{2}|\d{4}[-/]{2}\d{1,2}|\d{4}[-/]\d{1,2}[-/]{2}|[-/]{2}\d{1,2}|-?\d{1,2}[-/]\d{1,2}|\d{4}[-/]\d{1,2}[-/]\d{1,2}|-?\d{1,2}[-/]|[-/]{2})$";

        return Regex.IsMatch(trimmedDob, pattern);
    }

    private string EncryptCommaSeparatedIds(string? ids)
    {
        if (string.IsNullOrWhiteSpace(ids))
            return string.Empty;

        return string.Join(
            ",",
            ids.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(id => _stringCipher.Encrypt(id))
                .Where(encryptedId => !string.IsNullOrWhiteSpace(encryptedId)));
    }

    private TokenPairModel BuildTokenPair(dynamic cognitoResponse)
    {
        return new TokenPairModel
        {
            scheme = cognitoResponse?.tokenType,
            accessToken = cognitoResponse?.accessToken,
            refreshToken = cognitoResponse?.refreshToken,
            accessTokenExpiry = Convert.ToInt64(_configuration["AccessTokenExpiryTimeInMilliseconds"]),
            refreshTokenExpiry = Convert.ToInt64(_configuration["RefreshTokenExpiryInMilliseconds"]),
            isPasswordForceful = false
        };
    }

    private async Task<string?> RemoveUnconfirmedUsersOrGetDuplicateCodeAsync(
    string formattedMobileNumber,
    string cognitoUsername,
    string? actionIp,
    string actionPlatform)
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

    private void SafeRollbackTransaction()
    {
        try
        {
            _sqlTransactionHandler.RollbackTransaction();
        }
        catch (Exception ex)
        {
            SafeWriteExceptionLog(
                ex,
                _httpContextAccessor?.HttpContext?.GetIpAddress(),
                "reservations/customers");
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
    #endregion
    #endregion

    private async Task<(HttpStatusCode, object)> ValidationErrorResponseAsync(string errorCode)
    {
        return await BadRequestResponseAsync(
            ResponseCodes.Code400_BadRequest,
            new List<Errors>
            {
                new Errors
                {
                    errorCode = errorCode,
                    errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(errorCode)
                }
            }
        );
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

    private async Task<(HttpStatusCode, object)> SuccessResponseAsync(object data)
    {
        return (
            HttpStatusCode.OK,
            CommonApiResponseBuilder.Create()
                .WithCode(ResponseCodes.Code200_Success)
                .WithMessage(await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code200_Success))
                .WithData(data)
                .Build()
        );
    }

    private static string ResolveMessageCode(string? messageCode)
    {
        return string.IsNullOrWhiteSpace(messageCode)
            ? ResponseCodes.Code400_BadRequest
            : messageCode.Trim();
    }

    private string DecryptCustomerPreferencesOrEmpty(string? customerPreferencesJson)
    {
        if (string.IsNullOrWhiteSpace(customerPreferencesJson))
            return string.Empty;

        try
        {
            var fieldsToDecrypt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "preferenceOptionIds"
                };

            var decryptedJson = JsonHelper.DecryptFields(
                customerPreferencesJson,
                fieldsToDecrypt,
                raw => _commonAttributeStringCipher.Decrypt(raw),
                convertNumericStringToNumber: true
            );

            return JsonNode.Parse(decryptedJson)?.ToJsonString(new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = false
            }) ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private string EncryptCustomerPreferencesOrEmpty(string? customerPreferencesJson)
    {
        if (string.IsNullOrWhiteSpace(customerPreferencesJson))
            return string.Empty;

        try
        {
            var fieldsToDecrypt = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "preferenceOptionIds"
                };

            var decryptedJson = JsonHelper.DecryptFields(
                customerPreferencesJson,
                fieldsToDecrypt,
                raw => _commonAttributeStringCipher.Encrypt(raw),
                convertNumericStringToNumber: true
            );

            return JsonNode.Parse(decryptedJson)?.ToJsonString(new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = false
            }) ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
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
                propertiesJson = properties != null ? JsonSerializer.Serialize(properties) : null,
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
    private (
        string? TimeZoneOffsetValue,
        string? ActionIP,
        string ActionPlatform,
        CancellationToken CancellationToken
    ) GetRequestContext()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        var timeZoneOffsetValue = httpContext?.Items["TimezoneOffset"]?.ToString();
        var actionIP = httpContext?.GetIpAddress();

        var actionPlatform = "web";

        if (httpContext?.Request.Headers.TryGetValue("ActionPlatform", out var actionPlatformHeader) == true)
        {
            var headerValue = actionPlatformHeader.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(headerValue))
                actionPlatform = headerValue;
        }

        var cancellationToken = httpContext?.RequestAborted ?? CancellationToken.None;

        return (timeZoneOffsetValue, actionIP, actionPlatform, cancellationToken);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static bool IsDbSuccess(string? code)
    {
        return string.Equals(code, "0", StringComparison.Ordinal);
    }

    private static bool IsLoginPurpose(string? purpose)
    {
        return string.Equals(
            purpose,
            OtpPurposeConstants.Login,
            StringComparison.OrdinalIgnoreCase
        );
    }

    private static bool IsRegisterPurpose(string? purpose)
    {
        return string.Equals(
            purpose,
            OtpPurposeConstants.Register,
            StringComparison.OrdinalIgnoreCase
        );
    }

    private bool TryDecryptLong(string? encryptedValue, out long decryptedId)
    {
        decryptedId = 0;

        if (string.IsNullOrWhiteSpace(encryptedValue))
            return false;

        try
        {
            var decryptedValue = _stringCipher.Decrypt(encryptedValue);
            return long.TryParse(decryptedValue, out decryptedId);
        }
        catch
        {
            decryptedId = 0;
            return false;
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

    private string FormatPhoneNumber(string phoneNumber, string countryCode = "+977")
    {
        if (phoneNumber.StartsWith("0"))
            phoneNumber = phoneNumber.Substring(1);
        return $"{countryCode}{phoneNumber}";
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
}
