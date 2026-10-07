using hoslog.customer.business.CommonHandler;
using hoslog.customer.common.Search;
using hoslog.customer.domain.Search;
using hoslog.customer.shared;
using System.Net;
using hoslog.customer.common.Models.Search;
using hoslog.customer.common.Utilities;
using hoslog.customer.shared.Search;
using hoslog.customer.common;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using hoslog.customer.common.Models.Auth;
using hoslog.customer.common.Service;
using hoslog.customer.common.Models.RecommendationModel;
using hoslog.customer.domain.Recommendation;

namespace hoslog.customer.business.Search
{
    public class SearchBusiness : ISearchBusiness
    {
        private readonly IConfiguration _configuration;
        private ISearchRepository _searchRepository;
        private readonly StringCipher _stringCipher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CommonAttributeStringCipher _commonAttributeStringCipher;
        private readonly ApiResponseMessageService _apiResponseMessageService;
        private readonly IRecommendationRepository _recommendationRepository;
        public SearchBusiness(ISearchRepository searchRepository, StringCipher stringCipher, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, CommonAttributeStringCipher commonAttributeStringCipher, ApiResponseMessageService apiResponseMessageService, IRecommendationRepository recommendationRepository)
        {
            _searchRepository = searchRepository;
            _stringCipher = stringCipher;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _commonAttributeStringCipher = commonAttributeStringCipher;
            _apiResponseMessageService = apiResponseMessageService;
            _recommendationRepository = recommendationRepository;
        }

        public async Task<(HttpStatusCode, object)> GetHostDetailsAsync(string locationId, string searchByPreference, string searchByHeight, string searchByAge, string searchByBloodType, string searchByConstellation, int pageNo, AmazonSettingsCommon common)
        {
            var resp = new CommonApiResponse();
            var request = new SearchByHostDetail()
            {
                searchByPreference = searchByPreference,
                locationId = locationId,
                searchByHeight = searchByHeight,
                searchByAge = searchByAge,
                searchByConstellation = searchByConstellation,
                searchByBloodType = searchByBloodType,
                pageNo = pageNo
            };
            var response = await _searchRepository.SearchByHostDetailsAsync(request);
            if (response.Count > 0)
            {
                var respMapped = response.Select(item => new HostDetailsResponseModel
                {
                    clubId = _stringCipher.Encrypt(item.clubId),
                    hostId = _stringCipher.Encrypt(item.hostId),
                    hostName = item.hostName,
                    hostNameEng = item.hostNameEng,
                    clubName = item.clubNamme,
                    position = item.position,
                    otherPosition = item.otherPosition,
                    rowsTotal = item.totalCount,
                    rowNum = item.rowNum,
                    clubCategory = item.clubCategory,
                    hostThumbnail = new ImageResponse()
                    {
                        alt = "",
                        path = ImageHandler.ProcessedImage(common, item.hostThumbnail),
                        description = null
                    },
                    hostIcon = new ImageResponse()
                    {
                        alt = "",
                        path = ImageHandler.ProcessedImage(common, item.hostIcon),
                        description = null
                    },
                    hostImagePath = new ImageResponse()
                    {
                        alt = "",
                        path = ImageHandler.ProcessedImage(common, item.hostImagePath),
                        description = null
                    },
                    clubLogo = new ImageResponse()
                    {
                        alt = "",
                        path = ImageHandler.ProcessedImage(common, item.clubLogo),
                        description = null
                    },
                    clubCoverPhoto = new ImageResponse()
                    {
                        alt = "",
                        path = ImageHandler.ProcessedImage(common, item.clubCoverPhoto),
                        description = null
                    },
                    clubBusinessCertificate = new ImageResponse()
                    {
                        alt = "",
                        path = ImageHandler.ProcessedImage(common, item.clubBusinessCertificate),
                        description = null
                    }
                }).ToList();
                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetchedSuccess);
                resp.data = respMapped;
                return (HttpStatusCode.OK, resp);
            }
            resp.code = ResponseCodes.Code400_BadRequest;
            resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_RecordNotFound);
            resp.data = response;
            return (HttpStatusCode.BadRequest, resp);
        }

        public async Task<(HttpStatusCode, object)> SearchParticularAsync(string locationId, string searchByPreference, string fee, string searchByBusinessHour, string shop, int pageNo, string customerId, AmazonSettingsCommon common)
        {
            var resp = new CommonApiResponse();
            var request = new SearchByPreference()
            {
                locationId = locationId,
                searchByPreference = searchByPreference,
                fee = fee,
                searchByBusinessHour = searchByBusinessHour,
                shop = shop,
                pageNo = pageNo,
                customerId = customerId
            };
            var particularResponse = await _searchRepository.SearchByPreferenceAsync(request);
            if (particularResponse.Count > 0)
            {
                var respMapped = particularResponse.Select(item => new ClubListResponseModel
                {
                    clubId = item.clubId,
                    clubLocationId = item.ClubLocationId,
                    clubName = item.clubNameJapanese,
                    engClubName = item.clubNameEnglish,
                    groupName = item.groupName,
                    clubDescription = item.clubDescription,
                    tag1 = item.tag1,
                    tag2 = !string.IsNullOrEmpty(item.tag2) ? "ランク" + "" + item.tag2 : "",
                    tag3 = item.tag3,
                    tag4 = item.tag4,
                    tag5 = item.tag5,
                    clubOpeningTime = item.clubOpeningTime,
                    clubClosingTime = item.clubClosingTime,
                    clubTimePeriod = item.clubTimePeriod,
                    todaysClubSchedule = item.todaysClubSchedule,
                    locationURL = item.locationURL,
                    isBookmarked = item.isBookmarked,
                    totalComment = item.totalComment,
                    averageRating = item.averageRating,
                    clubLogo = new ImageResponse()
                    {
                        alt = "",
                        path = ImageHandler.ProcessedImage(common, item.clubLogo),
                        description = null
                    },
                    clubCoverPhoto = new ImageResponse()
                    {
                        alt = "",
                        path = ImageHandler.ProcessedImage(common, item.clubCoverPhoto),
                        description = null
                    },
                    imagePath = new ImageResponse()
                    {
                        alt = "",
                        path = ImageHandler.ProcessedImage(common, item.imagePath),
                        description = null
                    }
                }).ToList();
                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetchedSuccess);
                resp.data = respMapped;
                return (HttpStatusCode.OK, resp);
            }
            resp.code = ResponseCodes.Code400_BadRequest;
            resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_RecordNotFound);
            resp.data = Array.Empty<string>();
            return (HttpStatusCode.BadRequest, resp);
        }

        public async Task<(HttpStatusCode, object)> SearchByLocationAsync(string locationId, int pageNo, AmazonSettingsCommon common)
        {
            var resp = new CommonApiResponse();
            var request = new SearchByLocationModel()
            {
                locationName = locationId,
                timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty
            };
            var response = await _searchRepository.SearchByLocationAsync(request);
            if (response.Count > 0)
            {
                var respMapped = response.Select(item => new ClubListResponseModel
                {
                    clubId = _stringCipher.Encrypt(item.clubId),
                    clubLocationId = _stringCipher.Encrypt(item.ClubLocationId),
                    clubName = item.clubNameJapanese,
                    engClubName = item.clubNameEnglish,
                    groupName = item.groupName,
                    clubDescription = item.clubDescription,
                    tag1 = item.tag1,
                    tag2 = !string.IsNullOrEmpty(item.tag2) ? "ランク" + "" + item.tag2 : "",
                    tag3 = item.tag3,
                    tag4 = item.tag4,
                    tag5 = item.tag5,
                    clubOpeningTime = item.clubOpeningTime,
                    clubClosingTime = item.clubClosingTime,
                    clubTimePeriod = item.clubTimePeriod,
                    todaysClubSchedule = item.todaysClubSchedule,
                    locationURL = item.locationURL,
                    isBookmarked = item.isBookmarked,
                    totalComment = item.totalComment,
                    averageRating = item.averageRating,
                    clubCategory = item.clubCategory,
                    clubLogo = new ImageResponse()
                    {
                        alt = "",
                        path = ImageHandler.ProcessedImage(common, item.clubLogo),
                        description = null
                    },
                    clubCoverPhoto = new ImageResponse()
                    {
                        alt = "",
                        path = ImageHandler.ProcessedImage(common, item.clubCoverPhoto),
                        description = null
                    },
                    imagePath = new ImageResponse()
                    {
                        alt = "",
                        path = ImageHandler.ProcessedImage(common, item.imagePath),
                        description = null
                    }
                }).ToList();

                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetchedSuccess);
                resp.data = respMapped;
                return (HttpStatusCode.OK, resp);
            }
            resp.code = ResponseCodes.Code400_BadRequest;
            resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_RecordNotFound);
            resp.data = Array.Empty<string>();
            return (HttpStatusCode.BadRequest, resp);
        }

        public async Task<(HttpStatusCode, object)> SearchByParticularAsync(string locationId, string searchFilter, string price, string shift, string time, int pageNo, string customerId, string clubAvailability, AmazonSettingsCommon common)
        {
            var resp = new CommonApiResponse();
            var request = new SearchByParticular()
            {
                locationId = locationId,
                SearchFilter = searchFilter,
                price = price,
                shift = shift,
                time = time,
                pageNo = pageNo,
                customerId = customerId,
                clubAvailability = clubAvailability
            };
            if (request.shift != "3")
                request.time = "";
            var particularResponse = await _searchRepository.SearchByParticularAsync(request);
            if (particularResponse.Count > 0)
            {
                var respMapped = particularResponse.Select(item => new ClubListResponseModel
                {
                    clubId = _stringCipher.Encrypt(item.clubId),
                    clubLocationId = _stringCipher.Encrypt(item.ClubLocationId),
                    clubName = item.clubNameJapanese,
                    engClubName = item.clubNameEnglish,
                    groupName = item.groupName,
                    clubDescription = item.clubDescription,
                    tag1 = item.tag1,
                    tag2 = !string.IsNullOrEmpty(item.tag2) ? "ランク" + "" + item.tag2 : "",
                    tag3 = item.tag3,
                    tag4 = item.tag4,
                    tag5 = item.tag5,
                    clubOpeningTime = item.clubOpeningTime,
                    clubClosingTime = item.clubClosingTime,
                    clubTimePeriod = item.clubTimePeriod,
                    todaysClubSchedule = item.todaysClubSchedule,
                    locationURL = item.locationURL,
                    isBookmarked = item.isBookmarked,
                    totalComment = item.totalComment,
                    averageRating = item.averageRating,
                    clubCategory = item.clubCategory,
                    clubLogo = new ImageResponse()
                    {
                        alt = "",
                        path = ImageHandler.ProcessedImage(common, item.clubLogo),
                        description = null
                    },
                    clubCoverPhoto = new ImageResponse()
                    {
                        alt = "",
                        path = ImageHandler.ProcessedImage(common, item.clubCoverPhoto),
                        description = null
                    },
                    imagePath = new ImageResponse()
                    {
                        alt = "",
                        path = ImageHandler.ProcessedImage(common, item.imagePath),
                        description = null
                    }
                }).ToList();
                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetchedSuccess);
                resp.data = respMapped;
                return (HttpStatusCode.OK, resp);
            }
            resp.code = ResponseCodes.Code400_BadRequest;
            resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_RecordNotFound);
            resp.data = Array.Empty<string>();
            return (HttpStatusCode.BadRequest, resp);
        }

        public async Task<(HttpStatusCode, object)> ClubSearch(ClubSearchRequestCommon request)
        {
            var response = new CommonApiResponse
            {
                code = ResponseCodes.Code200_Success,
                message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code200_Success),
                data = new ClubSearchResponseModel()
            };

            var responseData = new ClubSearchResponseModel();
            var clubSearchRequest = request.MapObject<ClubSearchRequestCommon>();
            clubSearchRequest.planId = string.IsNullOrEmpty(clubSearchRequest.planId) ? null : _stringCipher.Decrypt(clubSearchRequest.planId);
            clubSearchRequest.locationId = string.IsNullOrEmpty(clubSearchRequest.locationId) ? null : _stringCipher.Decrypt(clubSearchRequest.locationId);
            if (_httpContextAccessor?.HttpContext?.User?.Claims.Count() > 0)
            {
                clubSearchRequest.customerId = _httpContextAccessor?.HttpContext?.User?.Claims?.First(x => x.Type == UserClaimsName.AgentId).Value ?? null;
                clubSearchRequest.customerId = string.IsNullOrEmpty(clubSearchRequest.customerId) ? null : _stringCipher.Decrypt(clubSearchRequest.customerId);
            }
            clubSearchRequest.timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty;
            var clubSearchResponse = await _searchRepository.ClubSearch(clubSearchRequest);
            if (!clubSearchResponse.Any())
                return (HttpStatusCode.OK, response);

            var amazonSettingsCommon = new AmazonSettingsCommon
            {
                BucketName = _configuration["BucketName"],
                BaseUrl = _configuration["BaseUrl"],
                ConfigAccess = _configuration["ConfigAccess"],
                ConfigSecret = _configuration["ConfigSecret"],
                ImageVirtualPath = _configuration["ImageVirtualPath"],
                PreviousImageUrl = _configuration["PreviousImageUrl"]
            };

            responseData.pagination = clubSearchResponse?.FirstOrDefault()?.MapObject<PaginationResponse>();
            responseData.clubCount = (int)responseData.pagination.totalRecords == null ? 0 : (int)responseData.pagination.totalRecords;
            responseData.clubList = clubSearchResponse.MapObjects<ClubSearchDetailModel>();

            var tasks = responseData.clubList.Select(async item =>
                {
                    var encryptedClubId = _stringCipher.Encrypt(item.clubId);
                    var clubLogo = clubSearchResponse.FirstOrDefault(c => c.clubId == item.clubId);
                    if (clubLogo != null)
                    {
                        item.recommendationClubLogo = new ClubSearchLogoModel
                        {
                            alt = clubLogo.clubLogoAlt,
                            description = clubLogo.clubLogoDescription,
                            path = ImageHandler.ProcessedImage(amazonSettingsCommon, clubLogo.clubLogoPath),
                        };
                    }
                    item.recommendationClubLogo.path = ImageHandler.ProcessedImage(amazonSettingsCommon, item.recommendationClubLogo.path);

                    var hostImages = await _searchRepository.GetHostImageViaClubId(item.clubId);
                    item.hostGalleryImage = hostImages.Select(item => new HostImageForClubCard
                    {
                        clubId = encryptedClubId,
                        hostId = string.IsNullOrEmpty(item.hostId) ? string.Empty : _stringCipher.Encrypt(item.hostId),
                        hostCode = item.hostCode,
                        imagePathResp = new ImageResponse
                        {
                            alt = "host_image",
                            description = "host image in club card",
                            path = string.IsNullOrEmpty(item.imagePath) ? string.Empty : ImageHandler.ProcessedImage(amazonSettingsCommon, item.imagePath)
                        }
                    }).ToList();
                    var clubPlanResponse = await _recommendationRepository.GetRecommendedClubPlan(item.clubId);
                    item.plans = clubPlanResponse == null ? new ClubSearchPlanModel() : clubPlanResponse.MapObject<ClubSearchPlanModel>();
                    item.plans.planId = _stringCipher.Encrypt(item?.plans?.planId);

                    item.clubId = encryptedClubId;
                    item.locationId = _stringCipher.Encrypt(item.locationId);
                });

            await Task.WhenAll(tasks);
            response.data = responseData;
            return (HttpStatusCode.OK, response);
        }

        public async Task<List<HostImageForClubCard>> GetHostImageViaClubId(string clubId)
        {
            return await _searchRepository.GetHostImageViaClubId(clubId);
        }
        public async Task<List<ClubSearchPlanCommon>> GetClubPlan(string clubId)
        {
            return await _searchRepository.GetClubPlan(clubId);
        }

        public async Task<(HttpStatusCode, object)> HostSearch(HostSearchRequestCommon request)
        {
            var response = new CommonApiResponse
            {
                code = ResponseCodes.Code200_Success,
                message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code200_Success),
                data = new HostSearchResponseModel()
            };
            var responseData = new HostSearchResponseModel();
            var hostSearchRequest = request.MapObject<HostSearchRequestCommon>();
            hostSearchRequest.locationId = string.IsNullOrEmpty(hostSearchRequest.locationId) ? null : _stringCipher.Decrypt(hostSearchRequest.locationId);
            if (_httpContextAccessor?.HttpContext?.User?.Claims.Count() > 0)
            {
                hostSearchRequest.customerId = _httpContextAccessor?.HttpContext?.User?.Claims?.First(x => x.Type == UserClaimsName.AgentId).Value ?? null;
                hostSearchRequest.customerId = string.IsNullOrEmpty(hostSearchRequest.customerId) ? null : _stringCipher.Decrypt(hostSearchRequest.customerId);
            }
            hostSearchRequest.constellation = string.IsNullOrEmpty(hostSearchRequest?.constellation) ? null : _commonAttributeStringCipher.Decrypt(hostSearchRequest?.constellation);
            if (!string.IsNullOrEmpty(hostSearchRequest?.bloodType))
            {
                var decryptedBloodTypes = hostSearchRequest.bloodType
                    .Split(',')
                    .Select(x => _commonAttributeStringCipher.Decrypt(x))
                    .ToList();

                hostSearchRequest.bloodType = decryptedBloodTypes.Any()
                    ? string.Join(",", decryptedBloodTypes)
                    : null;
            }
            else
                hostSearchRequest.bloodType = null;
            //hostSearchRequest.previousJob = string.IsNullOrEmpty(hostSearchRequest?.previousJob) ? null : _commonAttributeStringCipher.Decrypt(hostSearchRequest?.previousJob);
            hostSearchRequest.role = string.IsNullOrEmpty(hostSearchRequest?.role) ? null : _commonAttributeStringCipher.Decrypt(hostSearchRequest?.role);

            var hostSearchResponse = await _searchRepository.HostSearch(hostSearchRequest);
            if (!hostSearchResponse.Any())
                return (HttpStatusCode.OK, response);
            var amazonSettingsCommon = new AmazonSettingsCommon
            {
                BucketName = _configuration["BucketName"],
                BaseUrl = _configuration["BaseUrl"],
                ConfigAccess = _configuration["ConfigAccess"],
                ConfigSecret = _configuration["ConfigSecret"],
                ImageVirtualPath = _configuration["ImageVirtualPath"],
                PreviousImageUrl = _configuration["PreviousImageUrl"]
            };
            responseData.pagination = hostSearchResponse?.FirstOrDefault()?.MapObject<PaginationResponse>();
            responseData.hostCount = (int)responseData.pagination.totalRecords == null ? 0 : (int)responseData.pagination.totalRecords;
            responseData.hostDetail = hostSearchResponse.MapObjects<HostDetailModel>();
            responseData.hostDetail.ForEach(x =>
            {
                var hostLogoData = hostSearchResponse.FirstOrDefault(c => c.hostId == x.hostId);
                var clubLogoData = hostSearchResponse.FirstOrDefault(c => c.clubId == x.clubId);
                x.clubId = _stringCipher.Encrypt(x.clubId);
                x.hostId = _stringCipher.Encrypt(x.hostId);
                x.locationId = _stringCipher.Encrypt(x.locationId);
                x.clubLogo = ImageHandler.ProcessedImage(amazonSettingsCommon, x.clubLogo);
                x.hostImage = ImageHandler.ProcessedImage(amazonSettingsCommon, x.hostImage);
                x.recommendationHostImage = new HostSearchLogoModel
                {
                    description = hostLogoData?.clubLogoDescription,
                    alt = hostLogoData?.hostLogoAlt,
                    path = ImageHandler.ProcessedImage(amazonSettingsCommon, hostLogoData?.hostLogoPath)
                };
                x.recommendationClubLogo = new ClubSearchLogoModel
                {
                    alt = clubLogoData?.clubLogoAlt,
                    description = clubLogoData?.clubLogoDescription,
                    path = ImageHandler.ProcessedImage(amazonSettingsCommon, clubLogoData?.clubLogoPath)
                };
            });
            response.data = responseData;
            return (HttpStatusCode.OK, response);
        }
    }
}
