using hoslog.customer.common;
using hoslog.customer.common.Utilities;
using hoslog.customer.domain.Profile;
using hoslog.customer.shared;
using System.Net;
using hoslog.customer.shared.Profile;
using Microsoft.AspNetCore.Http;
using hoslog.customer.common.Models;
using hoslog.customer.business.CommonHandler;
using hoslog.customer.business.User;
using hoslog.customer.common.Middleware.AmazonCognito;
using hoslog.customer.common.Models.Auth;
using hoslog.customer.common.Service;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using hoslog.customer.common.Cache;
using Microsoft.Extensions.DependencyInjection;

namespace hoslog.customer.business.Profile
{
    public class ProfileBusiness : IProfileBusiness
    {
        private readonly IProfileRepository _profileRepository;
        private readonly CommonResponseHandler _commonResponseHandler;
        private readonly StringCipher _stringCipher;
        private readonly ValidationHelper _validationHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserBusiness _userBusiness;
        private readonly AmazonCognitoMiddleware _amazonCognitoMiddleware;
        private readonly SqlTransactionHandler _sqlTransactionHandler;
        private readonly CommonAttributeStringCipher _commonAttributeStringCipher;
        private readonly ApiResponseMessageService _apiResponseMessageService;
        private readonly IConfiguration _configuration;
        private readonly RedisCacheSetting _cacheSettings;
        private readonly IResponseCacheService _cacheService;
        private readonly AmazonSettingsCommon _amazonSettings;
        public ProfileBusiness(
            IProfileRepository profileRepository,
            StringCipher stringCipher,
            ValidationHelper validationHelper,
            IHttpContextAccessor httpContextAccessor,
            IUserBusiness userBusiness,
            AmazonCognitoMiddleware amazonCognitoMiddleware,
            SqlTransactionHandler sqlTransactionHandler,
            CommonAttributeStringCipher commonAttributeStringCipher,
            CommonResponseHandler commonResponseHandler,
            ApiResponseMessageService apiResponseMessageService,
            IConfiguration configuration,
            RedisCacheSetting cacheSettings,
            IServiceProvider serviceProvider
        )
        {
            _profileRepository = profileRepository;
            _commonResponseHandler = commonResponseHandler;
            _stringCipher = stringCipher;
            _validationHelper = validationHelper;
            _httpContextAccessor = httpContextAccessor;
            _userBusiness = userBusiness;
            _amazonCognitoMiddleware = amazonCognitoMiddleware;
            _sqlTransactionHandler = sqlTransactionHandler;
            _commonAttributeStringCipher = commonAttributeStringCipher;
            _apiResponseMessageService = apiResponseMessageService;
            _configuration = configuration;
            _cacheSettings = cacheSettings;
            _cacheService = cacheSettings.isEnabled
               ? serviceProvider.GetService<IResponseCacheService>()
               : null;

            _amazonSettings = new AmazonSettingsCommon
            {
                BucketName = _configuration["BucketName"],
                BaseUrl = _configuration["BaseUrl"],
                ConfigAccess = _configuration["ConfigAccess"],
                ConfigSecret = _configuration["ConfigSecret"],
                ImageVirtualPath = _configuration["ImageVirtualPath"],
                PreviousImageUrl = _configuration["PreviousImageUrl"]
            };
        }

        public async Task<(HttpStatusCode, object)> GetUserDetails(string userId, AmazonSettingsCommon common)
        {
            var resp = new CommonApiResponse()
            {
                errors = new List<Errors>()
            };
            var repoResp = await _profileRepository.GetUserDetails(_stringCipher.Decrypt(userId));

            if (repoResp == null || repoResp.Count <= 0)
            {
                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_Recordnotfound);
                resp.data = Array.Empty<string>();
                return (HttpStatusCode.OK, resp);
            }
            else
            {
                var prfList = repoResp.Select(prfs => new ProfileCommonResponse
                {
                    nickName = prfs.nickName,
                    firstName = prfs.firstName,
                    lastName = prfs.lastName,
                    mobileNumber = prfs.mobileNumber,
                    dob = prfs.dob,
                    emailAddress = prfs.emailAddress,
                    gender = prfs.gender,
                    preferredLocation = prfs.preferredLocation,
                    postalCode = prfs.postalCode,
                    prefecture = prfs.prefecture,
                    city = prfs.city,
                    street = prfs.street,
                    residenceNumber = prfs.residenceNumber,
                    balance = prfs.balance,
                    referCode = prfs.referCode,
                    image = new ImageResponse
                    {
                        alt = prfs.nickName,
                        path = ImageHandler.ProcessedImage(common, prfs.profileImage),
                        description = "user profile image"
                    },
                }).ToList();
                resp.data = prfList;
                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched);
                return (HttpStatusCode.OK, resp);
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


        public async Task<(HttpStatusCode, object)> UpdateUserDetails(UserRequestCommon userRequestCommon, AmazonSettingsCommon common)
        {
            var resp = new CommonApiResponse()
            {
                errors = new List<Errors>()
            };
            var (signatureValidationStatus, signatureValidationResponse) = await _validationHelper.ValidateSignatureAsync(userRequestCommon);
            if (signatureValidationStatus != HttpStatusCode.OK)
                return (signatureValidationStatus, signatureValidationResponse);

            var pattern = @"^(?:\d{4}[-/]{2}|\d{4}[-/]{2}\d{1,2}|\d{4}[-/]\d{1,2}[-/]{2}|[-/]{2}\d{1,2}|-?\d{1,2}[-/]\d{1,2}|\d{4}[-/]\d{1,2}[-/]\d{1,2}|-?\d{1,2}[-/]|[-/]{2})$";

            if (!string.IsNullOrWhiteSpace(userRequestCommon?.dob) &&
                userRequestCommon.dob.Trim() != "--" &&
                !Regex.IsMatch(userRequestCommon.dob.Trim(), pattern))
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

            //if (!DateTime.TryParse(userRequestCommon.dob, out DateTime dob))
            //{
            //    resp.code = ResponseCodes.Code400_BadRequest;
            //    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUE120_InvalidDateFormat.ToString());
            //    resp.data = Array.Empty<string>();
            //    resp.errors = new List<Errors>
            //        {
            //            new Errors { errorCode = ResponseCodes.CUE120_InvalidDateFormat,errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUE120_InvalidDateFormat.ToString()) }
            //        };
            //    return (HttpStatusCode.BadRequest, resp);
            //}

            //if (dob > DateTime.UtcNow)
            //{
            //    resp.code = ResponseCodes.Code400_BadRequest;
            //    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUE121_FutureDOB.ToString());
            //    resp.data = Array.Empty<string>();
            //    resp.errors = new List<Errors>
            //        {
            //            new Errors { errorCode = ResponseCodes.CUE121_FutureDOB,errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUE121_FutureDOB.ToString()) }
            //        };
            //    return (HttpStatusCode.BadRequest, resp);
            //}

            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(claims => claims.Type == UserClaimsName.UserId)?.Value;
            var agentId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(claims => claims.Type == UserClaimsName.AgentId)?.Value;

            userId = _stringCipher.Decrypt(userId) ?? string.Empty;
            agentId = _stringCipher.Decrypt(agentId) ?? string.Empty;

            var customerDetailResponse = await _userBusiness.GetCustomerDetail(new shared.User.UserDetailRequestCommon
            {
                customerId = agentId,
            });

            if (customerDetailResponse?.code != "0" || string.IsNullOrEmpty(customerDetailResponse?.NickName))
            {
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(customerDetailResponse?.message) ?? ResponseMessages.Msg400_BadRequest;
                return (HttpStatusCode.BadRequest, resp);
            }

            // bool needEmailUpdate = string.IsNullOrEmpty(customerDetailResponse?.EmailAddress) ? true : false;
            userRequestCommon.prefecture = string.IsNullOrEmpty(userRequestCommon.prefecture) ? null : _commonAttributeStringCipher.Decrypt(userRequestCommon.prefecture);
            userRequestCommon.dob = NormalizeDob(userRequestCommon?.dob);
            _sqlTransactionHandler.BeginTransaction();
            try
            {
                var repoResp = await _profileRepository.UpdateUserDetails(userRequestCommon, userId);
                if (repoResp.code != "0")
                {
                    resp.code = ResponseCodes.Code400_BadRequest;
                    resp.message = ResponseMessages.Msg400_BadRequest;
                    resp.data = Array.Empty<string>();
                    resp.errors = new List<Errors>
                    {
                        new Errors { errorCode = repoResp.message,errorMessage =await _apiResponseMessageService.GetMessageByCodeAsync(repoResp.message) }
                    };
                    _sqlTransactionHandler.RollbackTransaction();
                    return (HttpStatusCode.BadRequest, resp);
                }

                // if (needEmailUpdate && !string.IsNullOrEmpty(userRequestCommon?.email))
                // {
                //     var username = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(claims => claims.Type == UserClaimsName.Username)?.Value;
                //     username = _stringCipher.Decrypt(username) ?? string.Empty;

                //     var markEmailAsVerifiedResponse = await _amazonCognitoMiddleware.MarkEmailAsVerifiedAsync(new MarkEmailAsVerifiedModel.Request
                //     {
                //         Username = username,
                //         Email = userRequestCommon?.email
                //     });

                //     if (markEmailAsVerifiedResponse?.Code != ResponseCode.Success)
                //     {
                //         resp.code = ResponseCodes.Code400_BadRequest;
                //         //resp.message = markEmailAsVerifiedResponse?.Errors?.FirstOrDefault()?.ErrorMessage ?? ResponseMessages.Msg400_BadRequest;
                //         resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                //         resp.errors = new List<Errors>
                //         {
                //             new Errors
                //             {
                //                 errorCode = ResponseCodes.Code400_BadRequest,
                //                 errorMessage = markEmailAsVerifiedResponse.Errors?.FirstOrDefault()?.ErrorMessage
                //             }
                //         };
                //         _sqlTransactionHandler.RollbackTransaction();
                //         return (HttpStatusCode.BadRequest, resp);
                //     }
                // }

                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(repoResp.message);
                _sqlTransactionHandler.CommitTransaction();
                return (HttpStatusCode.OK, resp);
            }
            catch (Exception ex)
            {
                _sqlTransactionHandler.RollbackTransaction();
                throw new Exception("Error occurred while updating user details.", ex);
            }
        }
        public async Task<(HttpStatusCode, object)> SaveImage(string profileImage, string userId)
        {
            var resp = new CommonApiResponse()
            {
                errors = new List<Errors>()
            };
            userId = _stringCipher.Decrypt(userId);
            var repoResp = await _profileRepository.SaveImage(profileImage, userId);
            if (repoResp.code == "1")
            {
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = ResponseMessages.Msg400_BadRequest;
                resp.data = Array.Empty<string>();
                resp.errors = new List<Errors>
                 {
                   new Errors { errorCode = repoResp.message ,errorMessage =await _apiResponseMessageService.GetMessageByCodeAsync(repoResp.message) }
                 };
                return (HttpStatusCode.BadRequest, resp);
            }
            resp.code = ResponseCodes.Code200_Success;
            resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(repoResp.message);
            return (HttpStatusCode.OK, resp);
        }

        public async Task<(HttpStatusCode, object)> GetGalleryPhotoAsync(string clubId, string clubCode, int pageNo = 1)
        {
            string cacheKey = _cacheSettings.isEnabled ? _cacheService.GenerateCacheKey(
                     $"GetGalleryPhotoAsync:clubId={clubId ?? "null"}:clubCode={clubCode ?? "null"}"
                 ) : string.Empty;

            if (_cacheSettings.isEnabled && _cacheService != null)
            {
                var (cachedStatus, cachedValue) =
                    await _cacheService.GetCachedTupleResponseAsync<CommonApiResponse>(cacheKey);

                if (cachedStatus == HttpStatusCode.OK && cachedValue != null)
                    return (cachedStatus, cachedValue);
            }

            var requestModel = new PhotoGalleryRequestCommon
            {
                clubId = !string.IsNullOrEmpty(clubId) ? _stringCipher.Decrypt(clubId) : null,
                clubCode = clubCode,
                pageNo = pageNo
            };

            if (string.IsNullOrEmpty(requestModel.clubId) &&
                    string.IsNullOrEmpty(requestModel.clubCode))
            {
                return await _commonResponseHandler.HandleBadRequestResponse<CommonApiResponse>(responseDetailMessage: "One or more field validations failed",
                 fieldErrors: new() { new() { errorCode = ResponseCodes.CodeCUE104_ClubId_or_Clubcode_is_required, errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeCUE104_ClubId_or_Clubcode_is_required) } });
            }

            var recordNotFoundMsg = await _apiResponseMessageService
                .GetMessageByCodeAsync(ResponseCodes.CodeC702_Recordnotfound);

            var repoResponse = await _profileRepository.GetGalleryPhotoAsync(requestModel);
            var resp = new CommonApiResponse();

            if (!repoResponse.Any())
            {
                resp.code = ResponseCodes.Code200_Success;
                resp.message = recordNotFoundMsg;
                resp.data = Array.Empty<string>();
                return (HttpStatusCode.OK, resp);
            }

            var first = repoResponse.First();

            if (first.code == "1")
                return await _commonResponseHandler.HandleBadRequestResponse<CommonApiResponse>(
                    await _apiResponseMessageService.GetMessageByCodeAsync(first.message)
                );

            if (first.code == "0")
            {
                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched);
                resp.data = new PhotoGalleryResponseModel
                {
                    pagination = first.MapObject<PaginationResponse>(),
                    images = repoResponse.Select(item => new ImageResponse
                    {
                        alt = item.imageTitle,
                        path = ImageHandler.ProcessedImage(_amazonSettings, item.imagePath),
                        description = item.imageTitle
                    }).ToList()
                };

                if (_cacheSettings.isEnabled && _cacheService != null)
                {
                    await _cacheService.CacheTupleResponseAsync(
                        cacheKey,
                        HttpStatusCode.OK,
                        resp,
                        TimeSpan.FromMinutes(_cacheSettings.maximumIntervalInMinutes)
                    );
                }

                return (HttpStatusCode.OK, resp);
            }

            resp.code = ResponseCodes.Code200_Success;
            resp.message = !string.IsNullOrEmpty(first.message)
                ? await _apiResponseMessageService.GetMessageByCodeAsync(first.message)
                : recordNotFoundMsg;

            resp.data = Array.Empty<string>();

            return (HttpStatusCode.BadRequest, resp);
        }

        public async Task<(HttpStatusCode, object)> DeleteUserAsync(string userId)
        {
            var resp = new CommonApiResponse();
            var request = new CommonDBRequest
            {
                agentId = _stringCipher.Decrypt(_httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(claims => claims.Type == UserClaimsName.AgentId)?.Value),
                actionUserId = _stringCipher.Decrypt(_httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(claims => claims.Type == UserClaimsName.UserId)?.Value),
                actionIP = _httpContextAccessor.HttpContext.GetIpAddress(),
                actionPlatform = ""
            };
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
            _sqlTransactionHandler.BeginTransaction();
            try
            {
                var response = await _profileRepository.DeleteUserAsync(request);
                if (response.code == "1")
                {
                    resp.code = ResponseCodes.Code400_BadRequest;
                    resp.message = ResponseMessages.Msg400_BadRequest;
                    resp.errors = new List<Errors>
                    {
                    new Errors{
                               errorCode=response.message,
                               errorMessage=await _apiResponseMessageService.GetMessageByCodeAsync(response.message)
                              }
                    };
                    resp.data = Array.Empty<string>();
                    _sqlTransactionHandler.RollbackTransaction();
                    return (HttpStatusCode.BadRequest, resp);
                }
                var deleteResponse = await _amazonCognitoMiddleware.DeleteAccountAsync(new common.Models.Middleware.AmazonCognitoModel.UserManagement.DeleteAccountModel.Request
                {
                    AccessToken = token
                });
                if (deleteResponse?.Code != hoslog.customer.common.Models.Middleware.AmazonCognito.ResponseCode.Success)
                {
                    resp.code = ResponseCodes.Code400_BadRequest;
                    resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);
                    resp.errors = new List<Errors>
                    {
                        new Errors
                        {
                            errorCode = ResponseCodes.Code400_BadRequest,
                            errorMessage = deleteResponse?.Errors?.FirstOrDefault()?.ErrorMessage
                        }
                    };

                    _sqlTransactionHandler.RollbackTransaction();
                    return (HttpStatusCode.BadRequest, resp);
                }
                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(response.message);
                _sqlTransactionHandler.CommitTransaction();
                return (HttpStatusCode.OK, resp);
            }
            catch (Exception ex)
            {
                _sqlTransactionHandler.RollbackTransaction();
                throw new Exception("Error occurred while updating user details.", ex);
            }
        }
    }
}
