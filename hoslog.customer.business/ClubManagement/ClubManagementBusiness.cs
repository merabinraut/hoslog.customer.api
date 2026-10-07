using hoslog.customer.business.CommonHandler;
using hoslog.customer.common;
using hoslog.customer.common.Cache;
using hoslog.customer.common.Models.Auth;
using hoslog.customer.common.Models.ClubManagement;
using hoslog.customer.common.Service;
using hoslog.customer.common.Utilities;
using hoslog.customer.domain.ClubManagement;
using hoslog.customer.shared;
using hoslog.customer.shared.ClubManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
namespace hoslog.customer.business.ClubManagement
{
    public class ClubManagementBusiness : IClubManagementBusiness
    {
        private readonly IClubManagementRepository _clubRepository;
        private readonly StringCipher _stringCipher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CommonAttributeStringCipher _commonAttributeStringCipher;
        private readonly ApiResponseMessageService _apiResponseMessageService;
        private readonly RedisCacheSetting _cacheSettings;
        private readonly IResponseCacheService _cacheService;
        private readonly IConfiguration _configuration;
        private readonly AmazonSettingsCommon _amazonSettingsCommon;
        public ClubManagementBusiness(IClubManagementRepository clubRepository,
            StringCipher stringCipher,
            IHttpContextAccessor httpContextAccessor,
            CommonAttributeStringCipher commonAttributeStringCipher,
            ApiResponseMessageService apiResponseMessageService,
            RedisCacheSetting cacheSettings,
            IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            _clubRepository = clubRepository;
            _stringCipher = stringCipher;
            _httpContextAccessor = httpContextAccessor;
            _commonAttributeStringCipher = commonAttributeStringCipher;
            _apiResponseMessageService = apiResponseMessageService;
            _cacheSettings = cacheSettings;
            _configuration = configuration;
            _cacheService = cacheSettings.isEnabled
                ? serviceProvider.GetService<IResponseCacheService>()
                : null;
            _amazonSettingsCommon = new AmazonSettingsCommon
            {
                BucketName = _configuration["BucketName"] ?? string.Empty,
                BaseUrl = _configuration["BaseUrl"] ?? string.Empty,
                ConfigAccess = _configuration["ConfigAccess"] ?? string.Empty,
                ConfigSecret = _configuration["ConfigSecret"] ?? string.Empty,
                ImageVirtualPath = _configuration["ImageVirtualPath"] ?? string.Empty,
                PreviousImageUrl = _configuration["PreviousImageUrl"] ?? string.Empty
            };
        }

        public async Task<(HttpStatusCode, object)> GetClubDetailList(ClubDetailModel objClubDetailModel)
        {
            string cacheKey = _cacheSettings.isEnabled ? _cacheService.GenerateCacheKey(
                $"GetClubDetailList:search={objClubDetailModel.search ?? "null"}:pageNo={objClubDetailModel.pageNo?.ToString() ?? "null"}"
            ) : null;

            if (_cacheSettings.isEnabled && _cacheService != null)
            {
                var (cachedStatus, cachedValue) =
                    await _cacheService.GetCachedTupleResponseAsync<CommonApiResponse>(cacheKey);

                if (cachedStatus == HttpStatusCode.OK && cachedValue != null)
                    return (cachedStatus, cachedValue);
            }

            var response = new CommonApiResponse()
            {
                errors = new List<Errors>()
            };

            var repoResp = await _clubRepository.GetClubDetailList(objClubDetailModel);
            string message;
            var responseData = new ClubDetailResponseCommon();

            if (repoResp != null && repoResp.Any())
            {
                var firstClub = repoResp.FirstOrDefault();

                responseData.pagination = new PaginationResponse
                {
                    pageNo = objClubDetailModel.pageNo,
                    totalRecords = firstClub?.totalRecords ?? 0,
                    recordPerPage = firstClub?.recordPerPage ?? 0
                };

                responseData.clubList = repoResp.Select(club => new ClubDetailCommonData
                {
                    clubAgentId = _stringCipher.Encrypt(club.clubAgentId),
                    clubName = club.clubName,
                    clubNameEng = club.clubNameEng
                }).ToList();

                message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched);
            }
            else
                message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_Recordnotfound);

            response.data = responseData;
            response.code = ResponseCodes.Code200_Success;
            response.message = message;

            if (_cacheSettings.isEnabled && _cacheService != null)
            {
                await _cacheService.CacheTupleResponseAsync(
                    cacheKey,
                    HttpStatusCode.OK,
                    response,
                    TimeSpan.FromMinutes(_cacheSettings.maximumIntervalInMinutes));
            }
            return (HttpStatusCode.OK, response);
        }

        public async Task<(HttpStatusCode, object)> GetClubBasicInfoDetail(string clubId = "", string clubCode = "")
        {
            string? userId = null;
            string? clubAgentId = null;

            var claim = _httpContextAccessor.HttpContext?.User?
                   .Claims.FirstOrDefault(c => c.Type == UserClaimsName.UserId)?.Value;

            if (!string.IsNullOrEmpty(claim))
                userId = _stringCipher.Decrypt(claim);

            if (!string.IsNullOrEmpty(clubId))
                clubAgentId = _stringCipher.Decrypt(clubId);

            if (string.IsNullOrEmpty(clubAgentId) && string.IsNullOrEmpty(clubCode))
            {
                return (HttpStatusCode.BadRequest, new CommonApiResponse
                {
                    code = ResponseCodes.Code400_BadRequest,
                    message = ResponseMessages.Msg400_BadRequest,
                    data = Array.Empty<string>(),
                    errors = new List<Errors>
                    {
                        new Errors
                        {
                            errorCode = ResponseCodes.CodeCUE100_Invalid_request,
                            errorMessage = await _apiResponseMessageService
                                .GetMessageByCodeAsync(ResponseCodes.CodeCUE100_Invalid_request)
                        }
                    }
                });
            }

            string cacheKey = _cacheSettings.isEnabled ? _cacheService.GenerateCacheKey(
                           userId == null
                           ? $"GetClubBasicInfoDetail:clubId={clubId ?? "null"}:clubCode={clubCode ?? "null"}:guest"
                           : $"GetClubBasicInfoDetail:clubId={clubId ?? "null"}:clubCode={clubCode ?? "null"}:userId={userId}"
                       ) : null;

            if (_cacheSettings.isEnabled && _cacheService != null)
            {
                var (cachedStatus, cachedValue) =
                    await _cacheService.GetCachedTupleResponseAsync<CommonApiResponse>(cacheKey);

                if (cachedStatus == HttpStatusCode.OK && cachedValue != null)
                    return (cachedStatus, cachedValue);
            }
            var timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty;
            var dbResponse = await _clubRepository.GetClubBasicInformation(userId, clubAgentId, clubCode, timeZoneOffsetValue);
            if (dbResponse?.Any() != true)
            {
                return (HttpStatusCode.OK, new CommonApiResponse
                {
                    code = ResponseCodes.Code200_Success,
                    message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_Recordnotfound),
                    data = Array.Empty<string>()
                });
            }

            string requestClubId = dbResponse.First().clubId;
            if (string.IsNullOrEmpty(requestClubId))
            {
                return (HttpStatusCode.OK, new CommonApiResponse
                {
                    code = ResponseCodes.Code200_Success,
                    message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_Recordnotfound),
                    data = Array.Empty<string>()
                });
            }

            var validLabels = new List<string> { "最大予約人数", "ラストオーダー時間", "最終入店時間" };
            var planDetails = (await GetClubPlanDetails(requestClubId))
                ?.MapObjects<ClubPlanDetailModel>()
                ?.GroupBy(p => p.planName)
                .Select(g => new
                {
                    planName = g.Key,
                    details = g.Where(x => validLabels.Contains(x.label))
                               .Select(x => new { x.label, value = x.labelValue })
                });

            var ownPlanDetails = (await GetClubOwnPlanDetails(requestClubId))
                ?.MapObjects<ClubOwnPlanDetailModel>()
                ?.GroupBy(p => p.planName)
                .Select(g => new
                {
                    planName = g.Key,
                    details = g.Where(x => validLabels.Contains(x.label))
                               .Select(x => new { x.label, value = x.labelValue })
                });

            var amazonSettings = new AmazonSettingsCommon
            {
                BucketName = _configuration["BucketName"],
                BaseUrl = _configuration["BaseUrl"],
                ConfigAccess = _configuration["ConfigAccess"],
                ConfigSecret = _configuration["ConfigSecret"],
                ImageVirtualPath = _configuration["ImageVirtualPath"],
                PreviousImageUrl = _configuration["PreviousImageUrl"]
            };

            var clubBasicInfo = dbResponse.Select(c => new ClubBasicInformationResponseModel
            {
                clubId = _stringCipher.Encrypt(c.clubId),
                clubCode = c.clubCode,
                clubName = c.clubName,
                clubNameEng = c.clubNameEng,
                clubAddress = c.clubAddress,
                clubOpeningTime = c.clubOpeningTime,
                clubClosingTime = c.clubClosingTime,
                clubDescription = c.clubDescription,
                clubHoliday = c.clubHoliday,
                clubTimePeriod = c.clubTimePeriod,
                todaysClubSchedule = c.todaysClubSchedule,
                averageRating = c.averageRating,
                closingDate = c.closingDate,
                companionFee = c.companionFee,
                designationFee = c.designationFee,
                drinkFee = c.drinkFee,
                epLastEntryTime = c.epLastEntryTime,
                extensionFee = c.extensionFee,
                fullName = c.fullName,
                groupName = c.groupName,
                instagramURL = c.instagramURL,
                tiktokURL = c.tiktokURL,
                websiteURL = c.websiteURL,
                twitterURL = c.twitterURL,
                lineURL = c.lineURL,
                isBookmarked = c.isBookmarked,
                lastEntryTime = c.lastEntryTime,
                lastOrderTime = string.IsNullOrEmpty(c.lastOrderTime) ? null : c.lastOrderTime,
                locationId = _stringCipher.Encrypt(c.locationId),
                regularPrice = c.regularPrice,
                regularyEntry = c.regularyEntry,
                tag1 = c.tag1,
                tag2 = !string.IsNullOrEmpty(c.tag2) ? $"ランク{c.tag2}" : "",
                tag3 = c.tag3,
                tag4 = c.tag4,
                tag5 = c.tag5,
                tax = c.tax,
                totalComment = c.totalComment,
                mobileNumber = c.mobileNumber,
                landlineNumber = c.landlineNumber,
                clubCategory = c.clubCategory,
                reviewCount = Convert.ToInt32(c.reviewCount),
                hostCount = Convert.ToInt32(c.hostCount),
                galleryImageCount = Convert.ToInt32(c.galleryImageCount),
                clubLocationName = c.clubLocationName,
                location = new LocationResponse
                {
                    url = c.locationURL,
                    latitude = c.latitude,
                    longitude = c.longitude
                },
                resClubLogo = new ImageResponse
                {
                    alt = c.clubNameEng,
                    description = "club logo",
                    path = ImageHandler.ProcessedImage(amazonSettings, c.clubLogo)
                },
                resClubCoverPhoto = string.IsNullOrWhiteSpace(c.clubCoverPhoto)
                    ? null
                    : new ImageResponse
                    {
                        alt = c.clubNameEng,
                        description = "club cover image",
                        path = ImageHandler.ProcessedImage(amazonSettings, c.clubCoverPhoto)
                    },
                otherHoliday = string.IsNullOrEmpty(c.otherHoliday)
                    ? string.Empty
                    : string.Join(",", c.otherHoliday
                        .Split(',')
                        .Select(x => _commonAttributeStringCipher.Encrypt(x))),
                canReserve = c.canReserve
            }).ToList();

            var response = new CommonApiResponse
            {
                code = ResponseCodes.Code200_Success,
                message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched),
                data = new
                {
                    club_basic_info = clubBasicInfo,
                    plan_detail = planDetails,
                    club_own_plan_detail = ownPlanDetails
                }
            };

            if (_cacheSettings.isEnabled && _cacheService != null)
                await _cacheService.CacheTupleResponseAsync(cacheKey, HttpStatusCode.OK, response,
                    TimeSpan.FromMinutes(_cacheSettings.maximumIntervalInMinutes));

            return (HttpStatusCode.OK, response);
        }

        private async Task<IList<ClubOwnPlanDetailModel>?> GetClubOwnPlanDetails(string? clubId)
        {
            var dbResponse = await _clubRepository.GetClubOwnPlanDetails(clubId);
            return dbResponse;
        }

        public async Task<IList<ClubPlanDetailModel>> GetClubPlanDetails(string? clubId)
        {
            var dbResponse = await _clubRepository.GetClubPlanDetails(clubId);
            return dbResponse;
        }

        public async Task<(HttpStatusCode, object)> GetClubWeeklySchedule(string agentId = "", string clubCode = "", int duration = 1)
        {
            var response = new CommonApiResponse();
            agentId = !string.IsNullOrEmpty(agentId) ? _stringCipher.Decrypt(agentId) : string.Empty;
            if (string.IsNullOrEmpty(agentId) && string.IsNullOrEmpty(clubCode))
            {
                response.code = ResponseCodes.Code400_BadRequest;
                response.message = ResponseMessages.Msg400_BadRequest;
                response.errors = new List<Errors>
                {
                    new Errors{errorCode=ResponseCodes.CodeCUE101_Invalid_club,errorMessage=await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeCUE101_Invalid_club)   }
                };
                return (HttpStatusCode.BadRequest, response);
            }
            var clubScheduleResponse = await _clubRepository.GetClubWeeklySchedule(agentId, clubCode, duration);
            response.code = ResponseCodes.Code200_Success;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code200_Success);
            response.data = clubScheduleResponse.MapObjects<ClubScheduleModel>();
            return (HttpStatusCode.OK, response);
        }

        public async Task<(HttpStatusCode, object)> GetClubReviewAndRatings(AmazonSettingsCommon mappedAwsSetting, string clubId = "", string clubCode = "", int pageNo = 1)
        {
            var resp = new CommonApiResponse()
            {
                errors = new List<Errors>()
            };
            string? userId = string.Empty;
            string? clubAgentId = string.Empty;
            var claims = _httpContextAccessor.HttpContext?.User?.Claims;
            if (claims != null && claims.Count() > 0)
            {
                var claimValue = claims.First(x => x.Type == UserClaimsName.UserId)?.Value;
                userId = string.IsNullOrEmpty(claimValue) ? null : _stringCipher.Decrypt(claimValue);
            }
            if (!string.IsNullOrEmpty(clubId))
            {
                clubAgentId = _stringCipher.Decrypt(clubId);
            }
            var dbResponse = await _clubRepository.GetClubReviewAndRatings(clubAgentId, clubCode, pageNo);
            var userReviewInfo = dbResponse.MapObjects<ClubReviewsCommon>();
            if (dbResponse.Count() <= 0 || dbResponse == null)
            {
                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_Recordnotfound);
                resp.data = Array.Empty<string>();
                return (HttpStatusCode.OK, resp);
            }
            var pagination = new PaginationResponse()
            {
                pageNo = dbResponse.FirstOrDefault().pageNo,
                totalRecords = dbResponse.FirstOrDefault().totalRecords,
                recordPerPage = dbResponse.FirstOrDefault().recordPerPage,
            };
            userReviewInfo = dbResponse.Select(item => new ClubReviewsCommon
            {
                clubId = _stringCipher.Encrypt(item.clubId),
                reviewId = _stringCipher.Encrypt(item.reviewId),
                visitedDate = item.visitedDate,
                commentedOn = item.commentedOn,
                customerName = item.customerName,
                ratingScale = item.ratingScale,
                isCourseExtended = item.isCourseExtended,
                didMetFavourite = item.didMetFavourite,
                customerImageResponse = new ImageResponse
                {
                    alt = item.customerName,
                    description = "customer image",
                    path = ImageHandler.ProcessedImage(mappedAwsSetting, item.customerImage)
                },
                getClubReviewHostList = item.getClubReviewHostList.Select(host => new ClubReviewHostCommon
                {
                    hostId = _stringCipher.Encrypt(host.hostId),
                    hostCode = host.hostCode,
                    hostImageResponse = new ImageResponse
                    {
                        alt = host.hostName,
                        description = "host image",
                        path = ImageHandler.ProcessedImage(mappedAwsSetting, host.hostImage)
                    },
                    hostName = host.hostName,
                    isMVPHost = host.isMVPHost
                }).ToList(),
                getClubReviewRemarkList = item.getClubReviewRemarkList.Select(remark => new ClubReviewRemarkCommon
                {
                    englishRemark = remark.englishRemark,
                    japaneseRemark = remark.japaneseRemark,
                    remarkType = remark.remarkType
                }).ToList(),
                getIntentRemarks = item.getIntentRemarks.Select(intent => new IntentRemarks
                {
                    intentEnglishRemark = intent.intentEnglishRemark,
                    intentJapaneseRemark = intent.intentJapaneseRemark,
                    intentRemarkType = intent.intentRemarkType
                }).ToList(),
                qnaRemarks = item.qnaRemarks.Select(qnaRemarks => new QNARemarks
                {
                    qnaEngQuestion = qnaRemarks.qnaEngQuestion,
                    qnaJpQuestion = qnaRemarks.qnaJpQuestion,
                    qnaJpShortQuestion = qnaRemarks.qnaJpShortQuestion,
                    qnaEngAnswer = qnaRemarks.qnaEngAnswer,
                    qnaJpAnswer = qnaRemarks.qnaJpAnswer
                }).ToList(),
            }).ToList();
            resp.code = ResponseCodes.Code200_Success;
            resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code200_Success);
            resp.data = new
            {
                pagination = pagination,
                user_review_rating_info = userReviewInfo
            };
            return (HttpStatusCode.OK, resp);
        }

        #region shops you can vist now
        public async Task<(HttpStatusCode, CommonApiResponse)> ShopsYouCanVisitNow(string locationId, int pageNo = 1)
        {
            var response = new CommonApiResponse();
            locationId = !string.IsNullOrEmpty(locationId) ? _stringCipher.Decrypt(locationId) : string.Empty;
            var dbRequest = new ClubsYouCanVisitNowRequestCommon
            {
                pageNo = pageNo,
                locationId = locationId,
                timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty
            };
            var dbResponse = await _clubRepository.ShopsYouCanVisitNow(dbRequest);
            var responseData = new
            ClubsYouCanVisitNowResponseCommonModel
            {
                paginationResponse = new PaginationResponse
                {
                    pageNo = dbResponse?.FirstOrDefault()?.pageNo ?? 1,
                    totalRecords = dbResponse?.FirstOrDefault()?.totalRecords ?? 0,
                    recordPerPage = dbResponse?.FirstOrDefault()?.recordPerPage ?? 10,
                },
                clubList = dbResponse.Select(r => new ClubsYouCanVisitNowResponseModel
                {
                    clubId = _stringCipher.Encrypt(r?.clubId),
                    clubName = r.clubName,
                    clubNameJapanese = r.clubNameJapanese,
                    clubCode = r.clubCode,
                    locationId = _stringCipher.Encrypt(r?.locationId),
                    clubLogoResponse = new ImageResponse
                    {
                        path = ImageHandler.ProcessedImage(_amazonSettingsCommon, r.clubLogo),
                        alt = r.clubLogoAlt,
                        description = r.clubLogoDescription
                    }
                }).ToList()
            };

            response.code = ResponseCodes.Code200_Success;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code200_Success);
            response.data = responseData;
            return (HttpStatusCode.OK, response);
        }

        public async Task<(HttpStatusCode, CommonApiResponse)> ShopsYouCanVisitNowTest(string locationId, int pageNo = 1)
        {
            var response = new CommonApiResponse();
            // locationId = !string.IsNullOrEmpty(locationId) ? _stringCipher.Decrypt(locationId) : string.Empty;
            var dbRequest = new ClubsYouCanVisitNowRequestCommon
            {
                pageNo = pageNo,
                locationId = locationId,
                timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty
            };
            var dbResponse = await _clubRepository.ShopsYouCanVisitNow(dbRequest);
            var responseData = new
            ClubsYouCanVisitNowResponseCommonModel
            {
                paginationResponse = new PaginationResponse
                {
                    pageNo = dbResponse?.FirstOrDefault()?.pageNo ?? 1,
                    totalRecords = dbResponse?.FirstOrDefault()?.totalRecords ?? 0,
                    recordPerPage = dbResponse?.FirstOrDefault()?.recordPerPage ?? 10,
                },
                clubList = dbResponse.Select(r => new ClubsYouCanVisitNowResponseModel
                {
                    clubId = _stringCipher.Encrypt(r?.clubId),
                    clubName = r.clubName,
                    clubNameJapanese = r.clubNameJapanese,
                    clubCode = r.clubCode,
                    locationId = _stringCipher.Encrypt(r?.locationId),
                    clubLogoResponse = new ImageResponse
                    {
                        path = ImageHandler.ProcessedImage(_amazonSettingsCommon, r.clubLogo),
                        alt = r.clubLogoAlt,
                        description = r.clubLogoDescription
                    }
                }).ToList()
            };

            response.code = ResponseCodes.Code200_Success;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code200_Success);
            response.data = responseData;
            return (HttpStatusCode.OK, response);
        }
        #endregion
    }
}
