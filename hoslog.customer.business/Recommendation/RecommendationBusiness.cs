using System.Net;
using hoslog.customer.business.CommonHandler;
using hoslog.customer.common;
using hoslog.customer.common.Models;
using hoslog.customer.common.Models.Auth;
using hoslog.customer.common.Models.RecommendationModel;
using hoslog.customer.common.Service;
using hoslog.customer.common.Utilities;
using hoslog.customer.domain.Recommendation;
using hoslog.customer.shared;
using hoslog.customer.shared.Recommendation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.business.Recommendation;

public class RecommendationBusiness : IRecommendationBusiness
{
    private readonly IRecommendationRepository _repos;
    private readonly StringCipher _stringCipher;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApiResponseMessageService _apiResponseMessageService;
    private readonly IConfiguration _configuration;
    public RecommendationBusiness(IRecommendationRepository repos, StringCipher stringCipher, IHttpContextAccessor httpContextAccessor, ApiResponseMessageService apiResponseMessageService, IConfiguration configuration)
    {
        _repos = repos;
        _stringCipher = stringCipher;
        _httpContextAccessor = httpContextAccessor;
        _apiResponseMessageService = apiResponseMessageService;
        _configuration = configuration;
    }

    #region RECOMMENDATION HOST AND CLUB FOR HOME PAGE TAB
    public async Task<(HttpStatusCode, object)> GetHomepageRecommendationClubAndHostList(AmazonSettingsCommon common)
    {
        var response = new CommonApiResponse()
        {
            errors = new List<Errors>(),
        };
        string? userId = null;
        var claims = _httpContextAccessor.HttpContext?.User?.Claims;
        if (claims != null && claims.Count() > 0)
        {
            var claimValue = claims.First(x => x.Type == UserClaimsName.UserId)?.Value;
            userId = string.IsNullOrEmpty(claimValue) ? null : _stringCipher.Decrypt(claimValue);
        }
        RecommendationCommonModel commonModel = new RecommendationCommonModel();
        commonModel.RecommendationClub = await GetHomepageRecommendationClubList(userId);
        commonModel.RecommendationHost = await GetHomepageRecommendationHostList(userId);
        if (commonModel.RecommendationClub.Count() > 0 || commonModel.RecommendationHost.Count() > 0)
        {
            var recommendationClubMappedData = commonModel.RecommendationClub.MapObjects<RecommendationsClubModel>();
            var recommendationHostMappedData = commonModel.RecommendationHost.MapObjects<RecommendationsHostModel>();
            recommendationClubMappedData = commonModel.RecommendationClub.Select(clubItem => new RecommendationsClubModel
            {
                clubId = _stringCipher.Encrypt(clubItem.clubId),
                locationId = _stringCipher.Encrypt(clubItem.locationId),
                clubName = clubItem.clubName,
                clubNameJapanese = clubItem.clubNameJapanese,
                fullName = clubItem.fullName,
                groupName = clubItem.groupName,
                clubOpeningTime = clubItem.clubOpeningTime,
                clubClosingTime = clubItem.clubClosingTime,
                clubDescription = clubItem.clubDescription,
                clubCode = clubItem.clubCode,
                clubTimePeriod = clubItem.clubTimePeriod,
                averageRating = clubItem.averageRating,
                isBookmarked = clubItem.isBookmarked,
                locationURL = clubItem.locationURL,
                tag1 = clubItem.tag1,
                tag2 = !string.IsNullOrEmpty(clubItem.tag2) ? "ランク" + "" + clubItem.tag2 : "",
                tag3 = clubItem.tag3,
                tag4 = clubItem.tag4,
                tag5 = clubItem.tag5,
                todaysClubSchedule = clubItem.todaysClubSchedule,
                totalComment = clubItem.totalComment,
                clubCategory = clubItem.clubCategory,
                landline = clubItem.landline,
                hostGalleryImage = clubItem.hostGalleryImage
                                        .Select(item => new HostImageForClubCard
                                        {
                                            hostId = _stringCipher.Encrypt(item.hostId),
                                            imagePathResp = new ImageResponse
                                            {
                                                alt = "host_image",
                                                description = "host image in club card",
                                                path = ImageHandler.ProcessedImage(common, item.imagePath)
                                            }
                                        })
                                        .ToList(),
                recommendationClubLogo = new ImageResponse
                {
                    alt = clubItem.clubName,
                    description = "club logo",
                    path = ImageHandler.ProcessedImage(common, clubItem.clubLogo)
                },

            }).ToList();

            recommendationHostMappedData = commonModel.RecommendationHost.Select(hostItem => new RecommendationsHostModel
            {
                clubId = _stringCipher.Encrypt(hostItem.clubId),
                hostId = _stringCipher.Encrypt(hostItem.hostId),
                hostCode = hostItem.hostCode,
                hostName = hostItem.hostName,
                hostNameJapanese = hostItem.hostNameJapanese,
                hostBirthPlace = hostItem.hostBirthPlace,
                bloodType = hostItem.bloodType,
                clubCode = hostItem.clubCode,
                clubName = hostItem.clubName,
                clubNameJap = hostItem.clubNameJap,
                constellationGroup = hostItem.constellationGroup,
                dob = hostItem.dob,
                formattedDOB = hostItem.formattedDOB,
                height = hostItem.height,
                liquorStrength = hostItem.liquorStrength,
                locationId = _stringCipher.Encrypt(hostItem.locationId),
                locationURL = hostItem.locationURL,
                loveCount = hostItem.loveCount,
                previousOccupation = hostItem.previousOccupation,
                rank = hostItem.rank,
                isBookmarked = hostItem.isBookmarked,
                recommendationClubLogo = new ImageResponse
                {
                    alt = hostItem.hostName,
                    description = "club logo",
                    path = ImageHandler.ProcessedImage(common, hostItem.clubLogo)
                },
                recommendationHostImage = new ImageResponse
                {
                    alt = hostItem.hostName,
                    description = "host image",
                    path = ImageHandler.ProcessedImage(common, hostItem.hostImage)
                }
            }).ToList();
            response.code = ResponseCodes.Code200_Success;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUEREC103_HomepageRecommendedClubAndHostListFetchedSuccessfully);
            response.data = new
            {
                clubs = recommendationClubMappedData,
                hosts = recommendationHostMappedData
            };
            return (HttpStatusCode.OK, response);

        }
        else
        {
            response.code = ResponseCodes.Code200_Success;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_RecordNotFound);
            return (HttpStatusCode.OK, response);
        }
    }
    private async Task<IList<RecommendationClubCommonModel>> GetHomepageRecommendationClubList(string? userId)
    {
        var dbResponse = await _repos.GetHomepageRecommendationClubList(userId);
        return dbResponse;
    }
    private async Task<IList<RecommendationHostCommonModel>> GetHomepageRecommendationHostList(string? userId)
    {
        var dbResponse = await _repos.GetHomepageRecommendationHostList(userId);
        return dbResponse;
    }
    #endregion

    #region RECOMMENDATION HOST AND CLUB FOR OWL CARASOUL
    public async Task<(HttpStatusCode, object)> GetRecommendationClubList(AmazonSettingsCommon common)
    {
        var resp = new CommonApiResponse()
        {
            errors = new List<Errors>()
        };
        string? userId = null;
        var claims = _httpContextAccessor.HttpContext?.User?.Claims;
        if (claims != null && claims.Count() > 0)
        {
            var claimValue = claims.First(x => x.Type == UserClaimsName.UserId)?.Value;
            userId = string.IsNullOrEmpty(claimValue) ? null : _stringCipher.Decrypt(claimValue);
        }
        var response = await _repos.GetRecommendationClubList(userId);
        if (response != null)
        {
            var respMapped = response.MapObjects<RecommendationClubModel>();
            respMapped = respMapped.Select(item => new RecommendationClubModel
            {
                clubId = _stringCipher.Encrypt(item.clubId),
                locationId = _stringCipher.Encrypt(item.locationId),
                clubName = item.clubName,
                clubNameEng = item.clubNameEng,
                isBookmarked = item.isBookmarked,
                clubCategory = item.clubCategory,
                recommendedClubLogo = new ImageResponse()
                {
                    alt = item.clubName,
                    path = ImageHandler.ProcessedImage(common, item.clubLogo)
                }
            }).ToList();
            resp.code = ResponseCodes.Code200_Success;
            resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUEREC102_RecommendationClubListFetchedSuccessfully);
            resp.data = respMapped;
            return (HttpStatusCode.OK, resp);
        }
        else
        {
            resp.code = ResponseCodes.Code200_Success;
            resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_RecordNotFound);
            resp.data = Array.Empty<string>();
            return (HttpStatusCode.OK, resp);
        }
    }

    public async Task<(HttpStatusCode, object)> GetRecommendationHostList(AmazonSettingsCommon common)
    {
        var resp = new CommonApiResponse()
        {
            errors = new List<Errors>()
        };
        string? userId = null;
        var claims = _httpContextAccessor.HttpContext?.User?.Claims;
        if (claims != null && claims.Count() > 0)
        {
            var claimValue = claims.First(x => x.Type == UserClaimsName.UserId)?.Value;
            userId = string.IsNullOrEmpty(claimValue) ? null : _stringCipher.Decrypt(claimValue);
        }
        var response = await _repos.GetRecommendationHostList(userId);
        if (response.Count > 0)
        {
            var respMapped = response.MapObjects<RecommendationHostModel>();
            respMapped = response.Select(item => new RecommendationHostModel
            {
                clubId = _stringCipher.Encrypt(item.clubId),
                hostId = _stringCipher.Encrypt(item.hostId),
                locationId = _stringCipher.Encrypt(item.locationId),
                clubName = item.clubName,
                clubNameEng = item.clubNameEng,
                hostName = item.hostName,
                hostNameEng = item.hostNameEng,
                isBookmarked = item.isBookmarked,
                recommendedClubLogo = new ImageResponse()
                {
                    alt = item.clubName,
                    path = ImageHandler.ProcessedImage(common, item.clubLogo),
                    description = null
                },
                recommendedHostImage = new ImageResponse()
                {
                    alt = item.clubName,
                    path = ImageHandler.ProcessedImage(common, item.hostImage),
                    description = null,
                }
            }).ToList();
            resp.code = ResponseCodes.Code200_Success;
            resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUEREC100_RecommendationHostListFetchedSuccessfully);
            resp.data = respMapped;
            return (HttpStatusCode.OK, resp);
        }
        else
        {
            resp.code = ResponseCodes.Code200_Success;
            resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code200_Success);
            resp.data = Array.Empty<string>();
            return (HttpStatusCode.OK, resp);
        }
    }
    #endregion

    #region RECOMMENDATION HOST AND CLUB FOR MAIN PAGE TAB
    public async Task<(HttpStatusCode, object)> GetMainpageRecommendationClubAndHostList(AmazonSettingsCommon mappedAwsSetting, string? locationId, int? positionId)
    {
        var response = new CommonApiResponse()
        {
            errors = new List<Errors>(),
        };

        string? userId = _httpContextAccessor.HttpContext?.User?.Claims?
                .FirstOrDefault(x => x.Type == UserClaimsName.UserId)?.Value;

        if (!string.IsNullOrEmpty(userId))
            userId = _stringCipher.Decrypt(userId);

        if (!string.IsNullOrEmpty(locationId))
            locationId = _stringCipher.Decrypt(locationId);

        var commonModel = new RecommendationCommonModel
        {
            RecommendationClub = await GetMainpageRecommendationClubList(userId, locationId, positionId),
            RecommendationHost = await GetMainpageRecommendationHostList(userId, locationId, positionId)
        };

        var firstClub = commonModel.RecommendationClub.FirstOrDefault();
        int totalPosition = firstClub?.positionCount ?? 0;
        if (firstClub?.groupClubCount > 0 &&
        (commonModel.RecommendationClub.Any() || commonModel.RecommendationHost.Any()))
        {
            var encryptedClubs = commonModel.RecommendationClub.Select(club => new RecommendationsClubModel
            {
                clubId = _stringCipher.Encrypt(club.clubId),
                locationId = _stringCipher.Encrypt(club.locationId),
                clubName = club.clubName,
                clubNameJapanese = club.clubNameJapanese,
                clubCode = club.clubCode,
                clubOpeningTime = club.clubOpeningTime,
                clubClosingTime = club.clubClosingTime,
                clubDescription = club.clubDescription,
                clubTimePeriod = club.clubTimePeriod,
                todaysClubSchedule = club.todaysClubSchedule,
                fullName = club.fullName,
                groupName = club.groupName,
                isBookmarked = club.isBookmarked,
                locationURL = club.locationURL,
                averageRating = club.averageRating,
                tag1 = club.tag1 ?? string.Empty,
                tag2 = !string.IsNullOrEmpty(club.tag2) ? $"ランク{club.tag2}" : string.Empty,
                tag3 = club.tag3,
                tag4 = club.tag4,
                tag5 = club.tag5,
                totalComment = club.totalComment,
                totalPages = club.totalPages,
                clubCategory = club.clubCategory,
                landline = club.landline,
                hostGalleryImage = club.hostGalleryImage.Select(host => new HostImageForClubCard
                {
                    clubId = _stringCipher.Encrypt(club.clubId),
                    hostId = _stringCipher.Encrypt(host.hostId),
                    hostCode = host.hostCode,
                    imagePathResp = CreateImageResponse("host_image", "host image in club card", host.imagePath)
                }).ToList(),
                recommendationClubLogo = CreateImageResponse(club.clubName, "club logo image", club.clubLogo)
            }).ToList();

            var encryptedHosts = commonModel.RecommendationHost.Select(host => new RecommendationsHostModel
            {
                clubId = _stringCipher.Encrypt(host.clubId),
                locationId = _stringCipher.Encrypt(host.locationId),
                hostId = _stringCipher.Encrypt(host.hostId),
                hostName = host.hostName,
                hostNameJapanese = host.hostNameJapanese,
                clubName = host.clubName,
                clubNameJap = host.clubNameJap,
                bloodType = host.bloodType,
                clubCode = host.clubCode,
                constellationGroup = host.constellationGroup,
                dob = host.dob,
                formattedDOB = host.formattedDOB,
                height = host.height,
                hostBirthPlace = host.hostBirthPlace,
                hostCode = host.hostCode,
                isBookmarked = host.isBookmarked,
                liquorStrength = host.liquorStrength,
                locationURL = host.locationURL,
                loveCount = host.loveCount,
                previousOccupation = host.previousOccupation,
                rank = host.rank,
                totalPages = host.totalPages,
                recommendationHostImage = CreateImageResponse(host.hostName, "host image", host.hostImage),
                recommendationClubLogo = CreateImageResponse(host.clubName, "club logo", host.clubLogo)
            }).ToList();

            response.code = ResponseCodes.Code200_Success;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(
                ResponseCodes.CUEREC101_MainpageRecommendedClubAndHostListFetchedSuccessfully);

            response.data = new
            {
                totalPosition,
                clubs = encryptedClubs,
                hosts = encryptedHosts
            };

            return (HttpStatusCode.OK, response);
        }

        response.code = ResponseCodes.Code200_Success;
        response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_RecordNotFound);
        response.data = new
        {
            totalPosition,
            clubs = new List<object>(),
            hosts = new List<object>()
        };

        return (HttpStatusCode.OK, response);

        // Local helper methods
        ImageResponse CreateImageResponse(string alt, string description, string path) =>
            new ImageResponse
            {
                alt = alt,
                description = description,
                path = ImageHandler.ProcessedImage(mappedAwsSetting, path)
            };
    }

    public async Task<(HttpStatusCode, CommonApiResponse)> GetMainPageRecommendedClubList(string locationId, int positionId)
    {
        var amazonSettingsCommon = new AmazonSettingsCommon
        {
            BucketName = _configuration["BucketName"],
            BaseUrl = _configuration["BaseUrl"],
            ConfigAccess = _configuration["ConfigAccess"],
            ConfigSecret = _configuration["ConfigSecret"],
            ImageVirtualPath = _configuration["ImageVirtualPath"],
            PreviousImageUrl = _configuration["PreviousImageUrl"]
        };

        var response = new CommonApiResponse()
        {
            errors = new List<Errors>(),
        };

        string? userId = _httpContextAccessor.HttpContext?.User?.Claims?
                .FirstOrDefault(x => x.Type == UserClaimsName.UserId)?.Value;

        if (!string.IsNullOrEmpty(userId))
            userId = _stringCipher.Decrypt(userId);

        if (!string.IsNullOrEmpty(locationId))
            locationId = _stringCipher.Decrypt(locationId);

        var timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty;
        var dbReponse = await GetMainpageRecommendedClubList(userId, locationId, positionId, timeZoneOffsetValue);

        var firstClub = dbReponse.FirstOrDefault();
        int totalPosition = firstClub?.positionCount ?? 0;
        if (firstClub?.groupClubCount > 0 &&
        (dbReponse.Any() || dbReponse.Any()))
        {
            var encryptedClubs = dbReponse.Select(club => new RecommendedClubCommonModel
            {
                clubId = _stringCipher.Encrypt(club.clubId),
                locationId = _stringCipher.Encrypt(club.locationId),
                clubName = club.clubName,
                clubNameJapanese = club.clubNameJapanese,
                clubCode = club.clubCode,
                clubOpeningTime = club.clubOpeningTime,
                clubClosingTime = club.clubClosingTime,
                clubDescription = club.clubDescription,
                clubTimePeriod = club.clubTimePeriod,
                todaysClubSchedule = club.todaysClubSchedule,
                fullName = club.fullName,
                groupName = club.groupName,
                isBookmarked = club.isBookmarked,
                locationURL = club.locationURL,
                averageRating = club.averageRating,
                tag1 = club.tag1 ?? string.Empty,
                tag2 = !string.IsNullOrEmpty(club.tag2) ? $"ランク{club.tag2}" : string.Empty,
                tag3 = club.tag3,
                tag4 = club.tag4,
                tag5 = club.tag5,
                totalComment = club.totalComment,
                totalPages = club.totalPages,
                clubCategory = club.clubCategory,
                landline = club.landline,
                totalClubPlanCount = club.totalClubPlanCount,
                hostGalleryImage = club.hostGalleryImage.Select(host => new HostImageForClubCardModel
                {
                    clubId = _stringCipher.Encrypt(club.clubId),
                    hostId = _stringCipher.Encrypt(host.hostId),
                    hostCode = host.hostCode,
                    imagePathResp = CreateImageResponse("host_image", "host image in club card", host.imagePath)
                }).ToList(),
                recommendationClubLogo = CreateImageResponse(club.clubName, "club logo image", club.clubLogo)
            }).ToList();

            var tasks = encryptedClubs
                .Zip(dbReponse, (encrypted, raw) => (encrypted, raw))
                .Select(async pair =>
                {
                    var clubPlanResponse = await _repos.GetRecommendedClubPlan(pair.raw.clubId);

                    var plan = clubPlanResponse.MapObject<RecommendedClubPlanModel>();
                    if (plan != null && !string.IsNullOrWhiteSpace(plan.planId))
                    {
                        plan.planId = _stringCipher.Encrypt(plan.planId);
                    }

                    pair.encrypted.plans = plan;
                });

            await Task.WhenAll(tasks);


            response.code = ResponseCodes.Code200_Success;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(
                ResponseCodes.CUEREC101_MainpageRecommendedClubAndHostListFetchedSuccessfully);

            response.data = new
            {
                totalPosition,
                clubs = encryptedClubs
            };

            return (HttpStatusCode.OK, response);
        }
        response.code = ResponseCodes.Code200_Success;
        response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_RecordNotFound);
        response.data = new
        {
            totalPosition,
            clubs = new List<object>()
        };

        return (HttpStatusCode.OK, response);

        // Local helper methods
        ImageResponse CreateImageResponse(string alt, string description, string path) =>
            new ImageResponse
            {
                alt = alt,
                description = description,
                path = ImageHandler.ProcessedImage(amazonSettingsCommon, path)
            };
    }

    public async Task<(HttpStatusCode, CommonApiResponse)> GetMainPageRecommendedHostList(string locationId, int positionId)
    {
        var amazonSettingsCommon = new AmazonSettingsCommon
        {
            BucketName = _configuration["BucketName"],
            BaseUrl = _configuration["BaseUrl"],
            ConfigAccess = _configuration["ConfigAccess"],
            ConfigSecret = _configuration["ConfigSecret"],
            ImageVirtualPath = _configuration["ImageVirtualPath"],
            PreviousImageUrl = _configuration["PreviousImageUrl"]
        };

        var response = new CommonApiResponse()
        {
            errors = new List<Errors>(),
        };

        string? userId = _httpContextAccessor.HttpContext?.User?.Claims?
                .FirstOrDefault(x => x.Type == UserClaimsName.UserId)?.Value;

        if (!string.IsNullOrEmpty(userId))
            userId = _stringCipher.Decrypt(userId);

        if (!string.IsNullOrEmpty(locationId))
            locationId = _stringCipher.Decrypt(locationId);

        var dbReponse = await GetMainpageRecommendedHostList(userId, locationId, positionId);

        var firstClub = dbReponse.FirstOrDefault();
        int totalPosition = firstClub?.positionCount ?? 0;
        if (firstClub?.groupClubCount > 0 &&
        (dbReponse.Any() || dbReponse.Any()))
        {
            var encryptedHosts = dbReponse.Select(host => new RecommendedHostCommonModel
            {
                clubId = _stringCipher.Encrypt(host.clubId),
                locationId = _stringCipher.Encrypt(host.locationId),
                hostId = _stringCipher.Encrypt(host.hostId),
                hostName = host.hostName,
                hostNameJapanese = host.hostNameJapanese,
                clubName = host.clubName,
                clubNameJap = host.clubNameJap,
                bloodType = host.bloodType,
                clubCode = host.clubCode,
                constellationGroup = host.constellationGroup,
                dob = host.dob,
                formattedDOB = host.formattedDOB,
                height = host.height,
                hostBirthPlace = host.hostBirthPlace,
                hostCode = host.hostCode,
                isBookmarked = host.isBookmarked,
                liquorStrength = host.liquorStrength,
                locationURL = host.locationURL,
                loveCount = host.loveCount,
                previousOccupation = host.previousOccupation,
                rank = host.rank,
                totalPages = host.totalPages,
                recommendationHostImage = CreateImageResponse(host.hostName, "host image", host.hostImage),
                recommendationClubLogo = CreateImageResponse(host.clubName, "club logo", host.clubLogo)
            }).ToList();

            response.code = ResponseCodes.Code200_Success;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(
                ResponseCodes.CUEREC101_MainpageRecommendedClubAndHostListFetchedSuccessfully);

            response.data = new
            {
                totalPosition,
                hosts = encryptedHosts
            };

            return (HttpStatusCode.OK, response);
        }
        response.code = ResponseCodes.Code200_Success;
        response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_RecordNotFound);
        response.data = new
        {
            totalPosition,
            hosts = new List<object>()
        };
        return (HttpStatusCode.OK, response);

        // Local helper methods
        ImageResponse CreateImageResponse(string alt, string description, string path) =>
            new ImageResponse
            {
                alt = alt,
                description = description,
                path = ImageHandler.ProcessedImage(amazonSettingsCommon, path)
            };
    }

    private async Task<IList<RecommendationClubCommonModel>> GetMainpageRecommendationClubList(string? userId, string? locationId, int? positionId)
    {
        var dbResponse = await _repos.GetMainpageRecommendationClubList(userId, locationId, positionId);
        return dbResponse;
    }
    private async Task<IList<RecommendationHostCommonModel>> GetMainpageRecommendationHostList(string? userId, string? locationId, int? positionId)
    {
        var dbResponse = await _repos.GetMainpageRecommendationHostList(userId, locationId, positionId);
        return dbResponse;
    }

    private async Task<List<RecommendedClubCommon>> GetMainpageRecommendedClubList(string? userId, string? locationId, int? positionId, string timeZoneOffsetValue = "")
    {
        return await _repos.GetMainpageRecommendedClubList(userId, locationId, positionId, timeZoneOffsetValue);
    }
    private async Task<List<RecommendedHostCommon>> GetMainpageRecommendedHostList(string? userId, string? locationId, int? positionId)
    {
        return await _repos.GetMainpageRecommendedHostList(userId, locationId, positionId);
    }

    public async Task<(HttpStatusCode,object)> GetEditorsRecommendationClubList(string locationId,int? pageNo,int? pageSize)
    {
        var response = new CommonApiResponse()
        {
            errors = []
        };
        var amazonSettingsCommon = new AmazonSettingsCommon
        {
            BucketName = _configuration["BucketName"],
            BaseUrl = _configuration["BaseUrl"],
            ConfigAccess = _configuration["ConfigAccess"],
            ConfigSecret = _configuration["ConfigSecret"],
            ImageVirtualPath = _configuration["ImageVirtualPath"],
            PreviousImageUrl = _configuration["PreviousImageUrl"]
        };
        
        if(!string.IsNullOrEmpty(locationId))
            
            locationId = (_stringCipher.Decrypt(locationId));

        var dbResponse = await _repos.GetEditorsRecommendationClubList(locationId, pageNo, pageSize);
        
        var firstRecord = dbResponse.FirstOrDefault();
        var pagination = new PaginationResponse()
        {
            pageNo = pageNo,
            recordPerPage = pageSize,
            totalRecords = firstRecord?.totalData
        };
        if (dbResponse.Count > 0 && firstRecord.totalData > 0)
        {
            response.code = ResponseCodes.Code200_Success;
            response.message =
                await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched);
            var clubs = dbResponse.Select(s => new EditorRecommendationModel()
            {
                clubId = string.IsNullOrEmpty(s.clubId) ? "" : _stringCipher.Encrypt(s.clubId),
                groupName = s.groupName,
                groupNameJp = s.groupNameJp,
                locationId = string.IsNullOrEmpty(s.locationId) ? "" : _stringCipher.Encrypt(s.locationId),
                storeName = s.storeName,
                storeNameJp = s.storeNameJp,
                storeImage = new ImageResponse()
                {
                    alt = s.storeName,
                    description = "store image",
                    path = ImageHandler.ProcessedImage(amazonSettingsCommon, s.storeImagePath)
                }
            });
            response.data = new
            {
                pagination,
                clubs
            };
            return (HttpStatusCode.OK, response);

        }
        response.code = ResponseCodes.Code200_Success;
        response.message =
            await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_RecordNotFound);
        return (HttpStatusCode.OK, response);
    }   

    public async Task<(HttpStatusCode, object)> GetEditorsRecommendationClubDetailList(string? locationId, int? pageNo = 1, int? pageSize = 10)
    {
        var amazonSettingsCommon = new AmazonSettingsCommon
        {
            BucketName = _configuration["BucketName"],
            BaseUrl = _configuration["BaseUrl"],
            ConfigAccess = _configuration["ConfigAccess"],
            ConfigSecret = _configuration["ConfigSecret"],
            ImageVirtualPath = _configuration["ImageVirtualPath"],
            PreviousImageUrl = _configuration["PreviousImageUrl"]
        };

        var response = new CommonApiResponse()
        {
            errors = new List<Errors>(),
        };

        string? userId = _httpContextAccessor.HttpContext?.User?.Claims?
                .FirstOrDefault(x => x.Type == UserClaimsName.UserId)?.Value;

        if (!string.IsNullOrEmpty(userId))
            userId = _stringCipher.Decrypt(userId);

        if (!string.IsNullOrEmpty(locationId))
            locationId = _stringCipher.Decrypt(locationId);

        var timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty;
        var dbReponse = await _repos.GetEditorsRecommendationClubDetailList(userId, locationId, pageNo, pageSize, timeZoneOffsetValue);

        var firstClub = dbReponse.FirstOrDefault();
        if (firstClub?.code != "0")
        {
            return (HttpStatusCode.BadRequest,new CommonApiResponse{
               code = ResponseCodes.Code400_BadRequest,
               message = await _apiResponseMessageService.GetMessageByCodeAsync(firstClub?.message ?? "")
            });
        }
        var pagination = new PaginationResponse()
        {
            pageNo = pageNo,
            recordPerPage = pageSize,
            totalRecords = firstClub?.totalData
        };
        if (firstClub?.totalData > 0 &&
        (dbReponse.Any() || dbReponse.Any()))
        {
            var encryptedClubs = dbReponse.Select(club => new EditorRecommendationClubDetailListModel()
            {
                clubId = _stringCipher.Encrypt(club.clubId),
                locationId = _stringCipher.Encrypt(club.locationId),
                clubName = club.clubName,
                clubNameJapanese = club.clubNameJapanese,
                clubCode = club.clubCode,
                clubOpeningTime = club.clubOpeningTime,
                clubClosingTime = club.clubClosingTime,
                editorsDescription = club.editorsDescription,
                clubTimePeriod = club.clubTimePeriod,
                todaysClubSchedule = club.todaysClubSchedule,
                fullName = club.fullName,
                groupName = club.groupName,
                isBookmarked = club.isBookmarked,
                locationURL = club.locationURL,
                averageRating = club.averageRating,
                editorsTags = club.editorsTags,
                totalComment = club.totalComment,
                totalHosts = club.totalHosts,
                hostGalleryImage = club.hostGalleryImage.Select(host => new HostImageForClubCardModel
                {
                    clubId = _stringCipher.Encrypt(club.clubId),
                    hostId = _stringCipher.Encrypt(host.hostId),
                    hostCode = host.hostCode,
                    imagePathResp = new ImageResponse()
                    {
                        alt = host.hostId,
                        description = "host image in club card",
                        path = ImageHandler.ProcessedImage(amazonSettingsCommon, host.imagePath)
                    }
                }).ToList(),
                recommendationClubLogo = new ImageResponse()
                {
                    alt = club.clubName,
                    description = "club logo image",
                    path = ImageHandler.ProcessedImage(amazonSettingsCommon, club.clubLogo)
                }
            }).ToList();

            response.code = ResponseCodes.Code200_Success;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(
                ResponseCodes.CodeC701_RecordFetchedSuccess);

            response.data = new
            {
                pagination,
                clubs = encryptedClubs
            };

            return (HttpStatusCode.OK, response);
        }
        response.code = ResponseCodes.Code200_Success;
        response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_RecordNotFound);
        return (HttpStatusCode.OK, response);
    }

    #endregion
}
