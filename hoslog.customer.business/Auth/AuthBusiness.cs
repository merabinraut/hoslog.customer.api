using System.Diagnostics;
using hoslog.customer.business.User;
using hoslog.customer.common;
using hoslog.customer.common.Middleware;
using hoslog.customer.common.Middleware.AmazonCognito;
using hoslog.customer.common.Models.Auth;
using hoslog.customer.common.Models.Middleware.AmazonCognito;
using hoslog.customer.common.Models.Middleware.AmazonCognitoModel.Auth;
using hoslog.customer.common.Service;
using hoslog.customer.common.Utilities;
using hoslog.customer.domain.Auth;
using hoslog.customer.shared;
using hoslog.customer.shared.Auth;
using hoslog.customer.shared.User;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Reflection.Metadata;
using System.Text.Json;
using Azure;
using hoslog.customer.common.Models.Conversion;
using hoslog.customer.domain.Conversion;
using hoslog.customer.shared.Conversion;
using Microsoft.Extensions.Logging;

namespace hoslog.customer.business.Auth
{
    public class AuthBusiness : IAuthBusiness
    {
        private readonly IConfiguration _configuration;
        private readonly IConversionRepository _conversion;
        // private readonly AuthMiddleware _authMiddleware;
        private readonly ValidationHelper _validationHelper;
        private readonly ILogger<AuthBusiness> _logger;
        private readonly IAuthRepository _authRepository;
        private readonly IUserBusiness _userBusiness;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly StringCipher _stringCipher;
        private readonly AmazonCognitoMiddleware _amazonCognitoMiddleware;
        private readonly ApiResponseMessageService _apiResponseMessageService;
        private readonly SqlTransactionHandler _sqlTransactionHandler;
        public AuthBusiness(ILogger<AuthBusiness> logger,IConversionRepository conversion,IAuthRepository authRepository, ValidationHelper validationHelper, IUserBusiness userBusiness, IHttpContextAccessor httpContextAccessor, StringCipher stringCipher, IConfiguration configuration, AmazonCognitoMiddleware amazonCognitoMiddleware, ApiResponseMessageService apiResponseMessageService
            , SqlTransactionHandler sqlTransactionHandler)
        {
            _logger = logger;
            _conversion = conversion;
            _configuration = configuration;
            // _authMiddleware = authMiddleware;
            _authRepository = authRepository;
            _validationHelper = validationHelper;
            _userBusiness = userBusiness;
            _httpContextAccessor = httpContextAccessor;
            _stringCipher = stringCipher;
            _amazonCognitoMiddleware = amazonCognitoMiddleware;
            _apiResponseMessageService = apiResponseMessageService;
            _sqlTransactionHandler = sqlTransactionHandler;
        }

        public async Task<(HttpStatusCode, object)> Login(LoginRequestModel request,ConversionSessionContext session)
        {
            var validationResult = await _validationHelper.ValidateModelAndSignatureAsync(request, req => new LoginRequestModelValidator().Validate(req));
            if (validationResult.Item1 != HttpStatusCode.OK)
                return validationResult;

            var (isUserValid, user) = await ValidateUserAsync(request?.loginId, request?.password);
            if (!isUserValid || user == null)
                return await UnauthorizedResponse();

            //await _amazonCognitoMiddleware.AdminUserGlobalSignOutAsync(request?.loginId);

            var signInResponse = await _amazonCognitoMiddleware.SignInAsync(new SignInModel.Request
            {
                Username = user?.cognitoUsername.ToLower(),
                Password = request.password
            });

            if (signInResponse?.Code != ResponseCode.Success)
                return await UnauthorizedResponse();

            var responseData = signInResponse?.Data?.MapObject<SignInModel.Response>();

            var tokenPair = new TokenPairModel
            {
                scheme = responseData?.TokenType,
                accessToken = responseData?.AccessToken,
                refreshToken = responseData?.RefreshToken,
                accessTokenExpiry = Convert.ToInt64(_configuration["AccessTokenExpiryTimeInMilliseconds"]),
                refreshTokenExpiry = Convert.ToInt64(_configuration["RefreshTokenExpiryInMilliseconds"]),
                isPasswordForceful = user?.IsPasswordForceful ?? false
            };

            var userDetails = user?.MapObject<UserDetailCommon>();
            var manageTokenDBRequest = new ManageTokenRequestCommon
            {
                actionUserId = userDetails?.UserId,
                roleType = userDetails?.RoleType,
                accessToken = tokenPair.accessToken,
                accessTokenExpiryTimeMS = tokenPair.accessTokenExpiry,
                refreshToken = tokenPair.refreshToken,
                refreshTokenExpiryTimeMS = tokenPair.refreshTokenExpiry
            };

            var manageTokenDBResponse = await _authRepository.ManageToken(manageTokenDBRequest);
            if (manageTokenDBResponse?.code != "0")
                return await UnauthorizedResponse();
            
            
            if (!await ShouldBlockConversionFlowAsync(session))
            {
                try
                {
                    await _conversion.InsertEventAsync(new ConversionEventRequestCommon()
                    {
                        eventId = Guid.NewGuid(),
                        reservationJourneyId = GetJourneyGuid() ?? Guid.Empty,
                        conversionSessionSno = _stringCipher.Decrypt(session.conversionSessionSno),
                        eventName = "identity_resolved",
                        stepCode = "identity",
                        eventStatus = "success",
                        eventSource = "backend",
                        actorCustomerId = _stringCipher.Decrypt(session.currentCustomerId),
                        authenticationState = _stringCipher.Decrypt(session.currentCustomerId) != "0" ? "authenticated" : "guest",
                        relatedEntityType = "customer",
                        relatedEntityId = userDetails?.UserId.ToString(),
                        pageUrl = _httpContextAccessor?.HttpContext?.Request.Headers.Origin.ToString(),
                        timezoneOffsetMinutes =
                            _httpContextAccessor?.HttpContext?.Items["TimezoneOffsetMinutes"]?.ToString(),
                        previousPageUrl = _httpContextAccessor?.HttpContext?.Request.Headers.Referer.ToString(),
                        clientUTCDate = DateTime.UtcNow,
                        errorCategory = "",
                        errorCode = "",
                        errorMessageSafe = "",
                        propertiesJson = JsonSerializer.Serialize(request),
                        pageType = $"login_{request.loginId}",
                        requestTraceId = Activity.Current?.TraceId.ToString(),
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to insert conversion event");
                }
            }

            return (HttpStatusCode.OK, new CommonApiResponse
            {
                data = tokenPair,
                code = ResponseCodes.Code200_Success,
                message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code200_Success)
            });
        }

        public async Task<(HttpStatusCode, object)> Refresh(RefreshTokenRequestModel request)
        {
            var validationResult = await _validationHelper.ValidateModelAndSignatureAsync(request, req => new RefreshTokenRequestModelValidator().Validate(req));
            if (validationResult.Item1 != HttpStatusCode.OK)
                return validationResult;

            var (userStatusCode, user) = await _userBusiness.GetUserDetailViaUsername(request.username);
            if (userStatusCode != HttpStatusCode.OK || user == null)
                return await UnauthorizedResponse();

            var userDetail = user?.Adapt<CommonApiResponse>()?.data?.Adapt<UserDetailCommon>();

            var dbGetDetailRequest = new GetOrExpireTokenRequestCommon
            {
                actionUserId = userDetail?.UserId,
                roleType = userDetail?.RoleType
            };

            var getTokenDetails = await _authRepository.GetTokenDetail(dbGetDetailRequest);

            bool isValidRefreshToken = getTokenDetails != null
                && !string.IsNullOrEmpty(getTokenDetails.sno)
                && request.refreshToken == getTokenDetails.refreshToken;

            if (!isValidRefreshToken)
                return await UnauthorizedResponse();

            var refreshResponse = await _amazonCognitoMiddleware.RefreshAsync(new RefreshModel.Request
            {
                Username = userDetail?.cognitoUsername.ToLower(),
                RefreshToken = request?.refreshToken
            });

            if (refreshResponse?.Code != ResponseCode.Success)
                return await UnauthorizedResponse();

            var responseData = refreshResponse?.Data?.MapObject<RefreshModel.Response>();

            var tokenPair = new TokenPairModel
            {
                scheme = responseData?.TokenType,
                accessToken = responseData?.AccessToken,
                refreshToken = request?.refreshToken,
                accessTokenExpiry = Convert.ToInt64(_configuration["AccessTokenExpiryTimeInMilliseconds"]),
                refreshTokenExpiry = Convert.ToInt64(_configuration["RefreshTokenExpiryInMilliseconds"])
            };

            var manageTokenDBRequest = new ManageTokenRequestCommon
            {
                actionUserId = userDetail?.UserId,
                roleType = userDetail?.RoleType,
                accessToken = tokenPair.accessToken,
                accessTokenExpiryTimeMS = tokenPair.accessTokenExpiry,
                refreshToken = tokenPair.refreshToken,
                refreshTokenExpiryTimeMS = tokenPair.refreshTokenExpiry
            };

            var manageTokenDBResponse = await _authRepository.ManageToken(manageTokenDBRequest);
            if (manageTokenDBResponse?.code != "0")
                return await UnauthorizedResponse();

            return (HttpStatusCode.OK, new CommonApiResponse
            {
                code = ResponseCodes.Code200_Success,
                message = ResponseCodes.GetMessageForResponseCode(ResponseCodes.Code200_Success),
                data = tokenPair,
                errors = null
            });
        }

        public async Task<bool> ValidateTokenAsync(string accessToken, string userId, string roleType)
        {
            if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(roleType))
                return false;

            if (!await _amazonCognitoMiddleware.IsValidTokenAsync(accessToken))
                return false;

            var dbGetDetailRequest = new GetOrExpireTokenRequestCommon
            {
                actionUserId = userId,
                roleType = roleType
            };

            var getTokenDetails = await _authRepository.GetTokenDetail(dbGetDetailRequest);

            return getTokenDetails != null
                && !string.IsNullOrEmpty(getTokenDetails.sno)
                && accessToken == getTokenDetails.accessToken;
        }


        private async Task<(bool, UserDetailCommon)> ValidateUserAsync(string username, string password)
        {
            var (userStatusCode, user) = await _userBusiness.GetUserDetailViaUsername(username);
            if (userStatusCode == HttpStatusCode.OK && user != null)
            {
                var userDetailCommon = user.Adapt<CommonApiResponse>()?.data?.Adapt<UserDetailCommon>();
                if (userDetailCommon != null && SqlServerCrypto.PWDCOMPARE(password, userDetailCommon.Password))
                    return (true, userDetailCommon);
            }
            return (false, null);
        }


        private async Task<(HttpStatusCode, object)> UnauthorizedResponse()
        {
            return (HttpStatusCode.Unauthorized, new CommonApiResponse
            {
                data = null,
                code = ResponseCodes.Code401_Unauthorized,
                message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code401_Unauthorized.ToString())
            });
        }

        public async Task<(HttpStatusCode, object)> LogoutAsync()
        {
            var resp = new CommonApiResponse();
            string authorizationHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString();
            var token = authorizationHeader.StartsWith("Bearer ")
                   ? authorizationHeader.Substring("Bearer ".Length).Trim()
                   : null;
            if (string.IsNullOrEmpty(token))
            {
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = ResponseMessages.Msg400_BadRequest;
                return (HttpStatusCode.BadRequest, resp);
            }
            var response = await _amazonCognitoMiddleware.SignOutAsync(new SignOutModel.Request
            {
                AccessToken = token
            });
            if (response?.Code != ResponseCode.Success)
            {
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = response?.Errors?.FirstOrDefault()?.ErrorMessage ?? ResponseMessages.Msg400_BadRequest;
                return (HttpStatusCode.BadRequest, resp);
            }

            var dbExpireTokenRequest = new GetOrExpireTokenRequestCommon
            {
                actionUserId = _stringCipher.Decrypt(_httpContextAccessor.HttpContext?.User?.Claims.First(x => x.Type == UserClaimsName.UserId).Value ?? string.Empty),
                roleType = _stringCipher.Decrypt(_httpContextAccessor.HttpContext?.User?.Claims.First(x => x.Type == UserClaimsName.RoleType).Value ?? string.Empty),
                actionIP = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                actionPlatform = _httpContextAccessor?.HttpContext?.Request?.Headers["ActionPlatform"] ?? "WEB"
            };

            await _authRepository.ExpireToken(dbExpireTokenRequest);
       
            resp.code = ResponseCodes.Code200_Success;
            resp.message = ResponseMessages.Msg200_Success;
            resp.data = Array.Empty<string>();
            return (HttpStatusCode.OK, resp);
        }

        public Task<AuthDbResponse> CreateOtpAsync(
            string username,
            string purpose,
            string otpHash,
            string otpCode,
            string traceId,
            string cognitoAuthSession
        )
        {
            return _authRepository.CreateOtpAsync(
                username,
                purpose,
                otpHash,
                otpCode,
                traceId,
                cognitoAuthSession
            );
        }

        public Task<AuthDbResponse> VerifyOtpWithOutTransactionAsync(
            string username,
            string purpose,
            string otpCode,
            string submittedOtpHash = ""
        )
        {
            return _authRepository.VerifyOtpWithOutTransactionAsync(
                           username,
                           purpose,
                           otpCode,
                           submittedOtpHash
                       );
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
    
}