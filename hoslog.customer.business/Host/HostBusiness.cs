using hoslog.customer.business.CommonHandler;
using hoslog.customer.common;
using hoslog.customer.common.Host;
using hoslog.customer.common.Utilities;
using hoslog.customer.domain.Host;
using hoslog.customer.shared;
using hoslog.customer.shared.Host;
using Microsoft.AspNetCore.Http;
using System.Net;
using hoslog.customer.common.Models.Auth;
using hoslog.customer.common.Service;
using Microsoft.Extensions.Configuration;
using hoslog.customer.common.Cache;
using Microsoft.Extensions.DependencyInjection;
using hoslog.customer.common.Models.Host;
namespace hoslog.customer.business.Host
{
    public class HostBusiness : IHostBusiness
    {
        private readonly IHostRepository _hostRepository;
        private readonly ValidationHelper _validationHelper;
        private readonly StringCipher _stringCipher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CommonResponseHandler _commonResponseHandler;
        private readonly ApiResponseMessageService _apiResponseMessageService;
        private readonly IConfiguration _configuration;
        private readonly AmazonSettingsCommon _amazonSettingsCommon;
        private readonly RedisCacheSetting _cacheSettings;
        private readonly IResponseCacheService _cacheService;
        public HostBusiness(IHostRepository hostRepository,
            ValidationHelper validationHelper,
            StringCipher stringCipher,
            IHttpContextAccessor contextAccessor,
            CommonResponseHandler commonResponseHandler,
            ApiResponseMessageService apiResponseMessageService,
            IConfiguration configuration,
            RedisCacheSetting cacheSettings,
        IServiceProvider serviceProvider)
        {
            _hostRepository = hostRepository;
            _validationHelper = validationHelper;
            _stringCipher = stringCipher;
            _httpContextAccessor = contextAccessor;
            _commonResponseHandler = commonResponseHandler;
            _apiResponseMessageService = apiResponseMessageService;
            _configuration = configuration;
            _amazonSettingsCommon = new AmazonSettingsCommon
            {
                BucketName = _configuration["BucketName"] ?? string.Empty,
                BaseUrl = _configuration["BaseUrl"] ?? string.Empty,
                ConfigAccess = _configuration["ConfigAccess"] ?? string.Empty,
                ConfigSecret = _configuration["ConfigSecret"] ?? string.Empty,
                ImageVirtualPath = _configuration["ImageVirtualPath"] ?? string.Empty,
                PreviousImageUrl = _configuration["PreviousImageUrl"] ?? string.Empty
            };
            _cacheSettings = cacheSettings;
            _cacheService = cacheSettings.isEnabled
                   ? serviceProvider.GetService<IResponseCacheService>()
                   : null;
        }

        public async Task<(HttpStatusCode, object)> GetHostDetailsAsync(string hostId, string hostCode)
        {
            string decryptedHostId = _stringCipher.Decrypt(hostId);

            string userId = null;
            var claims = _httpContextAccessor.HttpContext?.User?.Claims;

            if (claims != null)
            {
                var encryptedUserId = claims
                    .FirstOrDefault(x => x.Type == UserClaimsName.UserId)?.Value;

                if (!string.IsNullOrEmpty(encryptedUserId))
                    userId = _stringCipher.Decrypt(encryptedUserId);
            }

            string cacheKey = _cacheSettings.isEnabled ? _cacheService.GenerateCacheKey(
                userId == null
                ? $"GetHostDetailsAsync:hostId={hostId ?? "null"}:hostCode={hostCode ?? "null"}:guest"
                : $"GetHostDetailsAsync:hostId={hostId ?? "null"}:hostCode={hostCode ?? "null"}:userId={userId}"
                ) : null;

            if (_cacheSettings.isEnabled && _cacheService != null)
            {
                var (cachedStatus, cachedValue) =
                    await _cacheService.GetCachedTupleResponseAsync<CommonApiResponse>(cacheKey);

                if (cachedStatus == HttpStatusCode.OK && cachedValue != null)
                    return (cachedStatus, cachedValue);
            }

            if (string.IsNullOrEmpty(decryptedHostId) && string.IsNullOrEmpty(hostCode))
            {
                return await _commonResponseHandler.HandleBadRequestResponse<CommonApiResponse>(
                    responseDetailMessage: "One or more field validations failed",
                    fieldErrors: new()
                    {
                        new()
                        {
                            errorCode = "HT100",
                            errorMessage = await _apiResponseMessageService
                                .GetMessageByCodeAsync(ResponseCodes.CodeCUE110_Host_id_or_host_code_is_required)
                        }
                    });
            }

            var (hostEntity, skills, personalities, images) =
                   await _hostRepository.GetHostDetailsAsync(decryptedHostId, hostCode, userId);

            if (hostEntity == null || string.IsNullOrEmpty(hostEntity.hostId))
            {
                var notFoundResponse = new CommonApiResponse
                {
                    code = ResponseCodes.Code400_BadRequest,
                    message = await _apiResponseMessageService
                        .GetMessageByCodeAsync(ResponseCodes.CodeC702_Recordnotfound),
                    data = Array.Empty<string>()
                };

                return (HttpStatusCode.BadRequest, notFoundResponse);
            }

            var responseDetails = hostEntity.MapObject<GetHostListDetailsCommon>();

            responseDetails.imageResponse ??= new List<ImageResponse>();

            responseDetails.hostId = _stringCipher.Encrypt(hostEntity.hostId);
            responseDetails.clubId = _stringCipher.Encrypt(hostEntity.clubId);

            responseDetails.clubLogoRes = new ImageResponse
            {
                alt = responseDetails.hostNameEnglish,
                path = ImageHandler.ProcessedImage(_amazonSettingsCommon, responseDetails.clubLogo),
                description = "host image"
            };

            responseDetails.skills = skills.MapObjects<Skills>();
            responseDetails.personality = personalities.MapObjects<Personality>();

            if (images != null)
            {
                foreach (var item in images)
                {
                    responseDetails.imageResponse.Add(new ImageResponse
                    {
                        alt = responseDetails.clubNameJapanese,
                        path = ImageHandler.ProcessedImage(_amazonSettingsCommon, item.imagePath),
                        description = null
                    });
                }
            }

            var resp = new CommonApiResponse
            {
                code = ResponseCodes.Code200_Success,
                message = await _apiResponseMessageService
                    .GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched),
                data = responseDetails
            };

            if (_cacheSettings.isEnabled && _cacheService != null)
            {
                await _cacheService.CacheTupleResponseAsync(
                    cacheKey,
                    HttpStatusCode.OK,
                    resp,
                    TimeSpan.FromMinutes(_cacheSettings.maximumIntervalInMinutes));
            }

            return (HttpStatusCode.OK, resp);
        }

        public async Task<(HttpStatusCode, object)> GetHostListAsync(string clubId, string clubCode)
        {
            string decryptedClubId = _stringCipher.Decrypt(clubId);

            string userId = null;
            var claims = _httpContextAccessor.HttpContext?.User?.Claims;

            if (claims != null && claims.Any())
            {
                var claimValue = claims.FirstOrDefault(x => x.Type == UserClaimsName.UserId)?.Value;
                userId = !string.IsNullOrEmpty(claimValue) ? _stringCipher.Decrypt(claimValue) : null;
            }

            string cacheKey = _cacheSettings.isEnabled ? _cacheService.GenerateCacheKey(
                userId == null
                ? $"GetHostListAsync:clubId={clubId ?? "null"}:clubCode={clubCode ?? "null"}:guest"
                : $"GetHostListAsync:clubId={clubId ?? "null"}:clubCode={clubCode ?? "null"}:userId={userId}"
            ) : null;

            if (_cacheSettings.isEnabled && _cacheService != null)
            {
                var (cachedStatus, cachedValue) =
                    await _cacheService.GetCachedTupleResponseAsync<CommonApiResponse>(cacheKey);

                if (cachedStatus == HttpStatusCode.OK && cachedValue != null)
                    return (cachedStatus, cachedValue);
            }

            if (string.IsNullOrEmpty(decryptedClubId) && string.IsNullOrEmpty(clubCode))
            {
                return await _commonResponseHandler.HandleBadRequestResponse<CommonApiResponse>(
                    responseDetailMessage: "One or more field validations failed",
                    fieldErrors: new()
                    {
                new() { errorCode = ResponseCodes.CodeCUE106_Club_Id_is_required, errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeCUE106_Club_Id_is_required) }
                    });
            }

            var requestCommon = new HostDetailRequestCommon
            {
                clubId = decryptedClubId,
                clubCode = clubCode,
                userId = userId
            };

            var response = await _hostRepository.GetHostListAsync(requestCommon);
            var dbResponse = await _hostRepository.GetRankedHostListAsync(requestCommon);

            List<HostDetailResponse> MapHostList(IEnumerable<HostDetailResponseCommon> hostList)
            {
                return hostList
                    .Where(item => item?.code == "0")
                    .Select(item => new HostDetailResponse
                    {
                        hostId = _stringCipher.Encrypt(item.hostId),
                        hostName = item.hostNameJapanese,
                        hostNameEng = item.hostName,
                        otherPosition = item.otherPosition,
                        hostCode = item.hostCode,
                        clubId = _stringCipher.Encrypt(item.clubId),
                        clubName = item.clubName,
                        clubCode = item.clubCode,
                        isBookMarked = item.isBookMarked,
                        hostTitle = item.hostTitle,
                        rank = item.rank,
                        image = new ImageResponse
                        {
                            alt = item.hostName,
                            path = ImageHandler.ProcessedImage(_amazonSettingsCommon, item.hostImage),
                            description = null
                        }
                    })
                    .ToList();
            }

            if (response.Any() || dbResponse.Any())
            {
                var resp = new CommonApiResponse
                {
                    code = ResponseCodes.Code200_Success,
                    message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched),
                    data = new
                    {
                        ranked_Host_List = MapHostList(dbResponse),
                        unranked_Host_List = MapHostList(response)
                    }
                };

                if (_cacheSettings.isEnabled && _cacheService != null)
                {
                    await _cacheService.CacheTupleResponseAsync(
                        cacheKey,
                        HttpStatusCode.OK,
                        resp,
                        TimeSpan.FromMinutes(_cacheSettings.maximumIntervalInMinutes));
                }

                return (HttpStatusCode.OK, resp);
            }
            else
            {
                var resp = new CommonApiResponse
                {
                    code = ResponseCodes.Code200_Success,
                    message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_Recordnotfound),
                    data = Array.Empty<string>()
                };
                return (HttpStatusCode.BadRequest, resp);
            }
        }

        public async Task<(HttpStatusCode, object)> GetScheduleListAsync(string clubId, string clubCode, long? date)
        {
            string cacheKey = _cacheSettings.isEnabled
                ? _cacheService.GenerateCacheKey($"GetScheduleListAsync:clubId={clubId ?? "null"}:clubCode={clubCode ?? "null"}:date={date?.ToString() ?? "null"}")
                : null;

            if (_cacheSettings.isEnabled && _cacheService != null)
            {
                var (cachedStatus, cachedValue) = await _cacheService.GetCachedTupleResponseAsync<CommonApiResponse>(cacheKey);

                if (cachedStatus == HttpStatusCode.OK && cachedValue != null)
                    return (cachedStatus, cachedValue);
            }

            var requesMapped = new ScheduleDetailsRequestCommon
            {
                clubId = _stringCipher.Decrypt(clubId),
                clubCode = clubCode,
                date = date,
                timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty
            };

            if (string.IsNullOrEmpty(requesMapped.clubId) && string.IsNullOrEmpty(clubCode))
            {
                var errorResponse = new CommonApiResponse
                {
                    code = ResponseCodes.Code400_BadRequest,
                    message = ResponseMessages.Msg400_BadRequest,
                    data = Array.Empty<string>(),
                    errors = new List<Errors>
                    {
                        new Errors
                        {
                            errorCode = ResponseCodes.CodeCUE101_Invalid_club,
                            errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeCUE101_Invalid_club)
                        }
                    }
                };
                return (HttpStatusCode.BadRequest, errorResponse);
            }

            var scheduleListResponse = await _hostRepository.GetScheduleListAsync(requesMapped);
            var resp = new CommonApiResponse();

            if (scheduleListResponse.Count > 0 && scheduleListResponse.FirstOrDefault().code.ToString().ToUpper() == "0")
            {
                var respMapped = scheduleListResponse.Select(item => new ScheduleResponse
                {
                    scheduleId = _stringCipher.Encrypt(item.scheduleId),
                    scheduleTitle = item.scheduleTitle,
                    scheduleDate = item.scheduleDate,
                    scheduleDescription = item.scheduleDescription,
                    eventType = _stringCipher.Encrypt(item.eventType),
                    closingDate = item.closingDate,
                    image = string.IsNullOrEmpty(item.scheduleImage)
                        ? new ImageResponse()
                        : new ImageResponse
                        {
                            alt = item.scheduleTitle,
                            path = ImageHandler.ProcessedImage(_amazonSettingsCommon, item.scheduleImage)
                        }
                }).ToList();

                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched);
                resp.data = respMapped;

                if (_cacheSettings.isEnabled && _cacheService != null)
                {
                    await _cacheService.CacheTupleResponseAsync(
                        cacheKey,
                        HttpStatusCode.OK,
                        resp,
                        TimeSpan.FromMinutes(_cacheSettings.minimumIntervalInMinutes));
                }

                return (HttpStatusCode.OK, resp);
            }

            resp.code = ResponseCodes.Code200_Success;
            resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_Recordnotfound);
            resp.data = Array.Empty<string>();
            return (HttpStatusCode.OK, resp);
        }

        public async Task<(HttpStatusCode, object)> GetHostsWithRankOrderingAsync(string clubId = null, string clubCode = null, int pageNo = 1, int pageSize = 12)
        {
            if (pageNo <= 0) pageNo = 1;

            string decryptedClubId = _stringCipher.Decrypt(clubId);

            string userId = null;
            var claims = _httpContextAccessor.HttpContext?.User?.Claims;

            if (claims != null && claims.Any())
            {
                var claimValue = claims.FirstOrDefault(x => x.Type == UserClaimsName.UserId)?.Value;
                userId = !string.IsNullOrEmpty(claimValue) ? _stringCipher.Decrypt(claimValue) : null;
            }

            if (string.IsNullOrEmpty(decryptedClubId) && string.IsNullOrEmpty(clubCode))
            {
                return await _commonResponseHandler.HandleBadRequestResponse<CommonApiResponse>(
                    responseDetailMessage: "One or more field validations failed",
                    fieldErrors: new()
                    {
                new() { errorCode = ResponseCodes.CodeCUE106_Club_Id_is_required, errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeCUE106_Club_Id_is_required) }
                    });
            }

            string cacheKey = _cacheSettings.isEnabled ? _cacheService.GenerateCacheKey(
                userId == null
                ? $"GetHostsWithRankOrderingAsync:clubId={clubId ?? "null"}:clubCode={clubCode ?? "null"}:pageNo={pageNo}:guest"
                : $"GetHostsWithRankOrderingAsync:clubId={clubId ?? "null"}:clubCode={clubCode ?? "null"}:pageNo={pageNo}:userId={userId}"
            ) : null;

            if (_cacheSettings.isEnabled && _cacheService != null)
            {
                var (cachedStatus, cachedValue) =
                    await _cacheService.GetCachedTupleResponseAsync<CommonApiResponse>(cacheKey);

                if (cachedStatus == HttpStatusCode.OK && cachedValue != null)
                    return (cachedStatus, cachedValue);
            }

            var requestCommon = new HostListRequestCommon
            {
                clubId = decryptedClubId,
                clubCode = clubCode,
                userId = userId,
                pageNo = pageNo,
                pageSize = pageSize
            };

            var dbResponse = await _hostRepository.HostsWithRankOrdering(requestCommon);

            if (dbResponse == null || dbResponse?.FirstOrDefault()?.code != "0")
            {
                return (HttpStatusCode.BadRequest, new CommonApiResponse
                {
                    code = ResponseCodes.Code200_Success,
                    message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_Recordnotfound),
                    data = Array.Empty<string>()
                });
            }

            var responseData = new HostsWithRankOrderingResponseModel();

            var hostList = dbResponse
                   .Where(item => item?.code == "0")
                   .Select(item => new HostsWithRankOrderingModel
                   {
                       clubId = _stringCipher.Encrypt(item?.clubId),
                       hostId = _stringCipher.Encrypt(item?.hostId),
                       hostCode = item.hostCode,
                       hostName = item.hostNameJapanese,
                       position = item.position,
                       isBookMarked = item.isBookMarked,
                       hostTitle = item.hostTitle,
                       rank = item.rank,
                       image = new ImageResponse
                       {
                           alt = item.hostName,
                           path = ImageHandler.ProcessedImage(_amazonSettingsCommon, item.hostImage),
                           description = item.hostName
                       }
                   })
                   .ToList();

            var resp = new CommonApiResponse
            {
                code = ResponseCodes.Code200_Success,
                message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched),
                data = new
                {
                    paginationResponse = new PaginationResponse
                    {
                        pageNo = dbResponse?.FirstOrDefault()?.pageNo,
                        totalRecords = dbResponse?.FirstOrDefault()?.totalRecords,
                        recordPerPage = dbResponse?.FirstOrDefault()?.recordPerPage,
                    },
                    hostList = hostList.MapObjects<HostsWithRankOrderingModel>()
                }
            };

            if (_cacheSettings.isEnabled && _cacheService != null)
            {
                await _cacheService.CacheTupleResponseAsync(
                    cacheKey,
                    HttpStatusCode.OK,
                    resp,
                    TimeSpan.FromMinutes(_cacheSettings.maximumIntervalInMinutes));
            }

            return (HttpStatusCode.OK, resp);
        }

        public async Task<(HttpStatusCode, object)> GetHostDetailsV2Async(string hostId, long hostCode)
        {
            long decryptedHostId = string.IsNullOrEmpty(hostId) ? 0 : long.Parse(_stringCipher.Decrypt(hostId));

            if (decryptedHostId <= 0 && hostCode <= 0)
            {
                return await _commonResponseHandler.HandleBadRequestResponse<CommonApiResponse>(
                    responseDetailMessage: "One or more field validations failed",
                    fieldErrors: new()
                    {
                        new()
                        {
                            errorCode = "HT100",
                            errorMessage = await _apiResponseMessageService
                                .GetMessageByCodeAsync(ResponseCodes.CodeCUE110_Host_id_or_host_code_is_required)
                        }
                    });
            }

            long userId = 0;
            var claims = _httpContextAccessor.HttpContext?.User?.Claims;

            if (claims != null)
            {
                var encryptedUserId = claims
                    .FirstOrDefault(x => x.Type == UserClaimsName.UserId)?.Value;

                if (!string.IsNullOrEmpty(encryptedUserId))
                    userId = long.Parse(_stringCipher.Decrypt(encryptedUserId));
            }

            var (hostDetailsDBResponse, hostImagesDBResponse) = await _hostRepository.GetHostDetailsV2Async(decryptedHostId, hostCode, userId);

            if (hostDetailsDBResponse == null || hostDetailsDBResponse.code != "0")
            {
                var notFoundResponse = new CommonApiResponse
                {
                    code = ResponseCodes.Code400_BadRequest,
                    message = string.IsNullOrEmpty(hostDetailsDBResponse?.message) ? await _apiResponseMessageService
                        .GetMessageByCodeAsync(ResponseCodes.CodeC702_Recordnotfound) : await _apiResponseMessageService
                        .GetMessageByCodeAsync(hostDetailsDBResponse?.message),
                    data = Array.Empty<string>()
                };

                return (HttpStatusCode.BadRequest, notFoundResponse);
            }

            List<HostPhotoV2Model> BuildPhotos(List<HostImagesV2Common>? images)
            {
                if (images == null || images.Count == 0 || images.FirstOrDefault()?.code != "0")
                    return new List<HostPhotoV2Model>();

                return images
                    .Where(x => !string.IsNullOrWhiteSpace(x.imagePath))
                    .Select((x, index) => new HostPhotoV2Model
                    {
                        id = string.IsNullOrWhiteSpace(x.id) ? $"p{index + 1}" : _stringCipher.Encrypt(x.id),
                        url = ImageHandler.ProcessedImage(_amazonSettingsCommon, x.imagePath)
                    })
                    .ToList();
            }

            string? BuildHostShareUrl(
                string? template,
                HostDetailsV2Common? hostDetails)
            {
                if (string.IsNullOrWhiteSpace(template) || hostDetails == null)
                    return null;

                return template
                    .Replace("{locationMappingURL}", hostDetails.locationMappingURL ?? string.Empty)
                    .Replace("{clubCode}", hostDetails.clubCode ?? string.Empty)
                    .Replace("{hostCode}", hostDetails.hostCode ?? string.Empty);
            }

            var hostShareUrlTemplate = _configuration["HostShareURL"];

            var responseData = new HostDetailsV2ResponseModel
            {
                store = new StoreV2Model
                {
                    id = _stringCipher.Encrypt(hostDetailsDBResponse?.clubId.ToString()),
                    name = hostDetailsDBResponse?.clubName,
                    nameKana = hostDetailsDBResponse?.clubNameKatakana,
                    area = hostDetailsDBResponse?.clubLocationName,
                    logoUrl = ImageHandler.ProcessedImage(_amazonSettingsCommon, hostDetailsDBResponse?.clubLogo)
                },
                host = new HostV2Model
                {
                    id = _stringCipher.Encrypt(hostDetailsDBResponse?.hostId.ToString()),
                    rank = hostDetailsDBResponse?.hostRank ?? 0,
                    name = hostDetailsDBResponse?.hostNameJapanese,
                    nameRoman = hostDetailsDBResponse?.hostName,
                    role = hostDetailsDBResponse?.hostRole,
                    badge = hostDetailsDBResponse?.hostBadge,
                    isHostBookmarked = hostDetailsDBResponse?.isHostBookmarked ?? false,
                    stats = new HostStatsV2Model
                    {
                        likes = hostDetailsDBResponse?.hostLikes ?? 0,
                        reviews = hostDetailsDBResponse?.hostReviews ?? 0
                    },
                    socialLinks = new HostSocialLinksV2Model
                    {
                        instagram = hostDetailsDBResponse?.hostInstagramLink,
                        tiktok = hostDetailsDBResponse?.hostTiktokLink,
                        x = hostDetailsDBResponse?.hostTwitterLink,
                        line = hostDetailsDBResponse?.hostLineLink,
                        shareUrl = BuildHostShareUrl(hostShareUrlTemplate, hostDetailsDBResponse)
                    },
                    profile = new HostProfileV2Model
                    {
                        birthday = hostDetailsDBResponse?.hostDOB,
                        heightCm = hostDetailsDBResponse?.hostHeight ?? "0",
                        zodiac = hostDetailsDBResponse?.hostZodiac,
                        birthplace = hostDetailsDBResponse?.hostBirthPlace,
                        bloodType = hostDetailsDBResponse?.hostBloodType,
                        mbti = hostDetailsDBResponse?.hostMbtiTypeCode
                    },
                    photos = BuildPhotos(hostImagesDBResponse),
                    hostingStyle = new HostingStyleV2Model
                    {
                        appearance = int.TryParse(hostDetailsDBResponse?.appearance, out int appearance) ? appearance : 0,
                        conversationStyle = int.TryParse(hostDetailsDBResponse?.conversationStyle, out int conversationStyle) ? conversationStyle : 0,
                        alcoholTolerance = int.TryParse(hostDetailsDBResponse?.alcoholTolerance, out int alcoholTolerance) ? alcoholTolerance : 0,
                        atmosphere = int.TryParse(hostDetailsDBResponse?.atmosphere, out int atmosphere) ? atmosphere : 0
                    }
                }
            };

            var resp = new CommonApiResponse
            {
                code = ResponseCodes.Code200_Success,
                message = await _apiResponseMessageService
                   .GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched),
                data = responseData
            };
            return (HttpStatusCode.OK, resp);
        }
    }
}
