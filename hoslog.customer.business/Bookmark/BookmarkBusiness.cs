using System.Net;
using hoslog.customer.business.CommonHandler;
using hoslog.customer.common;
using hoslog.customer.common.Models.Auth;
using hoslog.customer.common.Models.BookmarkModel;
using hoslog.customer.common.Models.RecommendationModel;
using hoslog.customer.common.Service;
using hoslog.customer.common.Utilities;
using hoslog.customer.domain.Bookmark;
using hoslog.customer.domain.Recommendation;
using hoslog.customer.shared;
using Microsoft.AspNetCore.Http;

namespace hoslog.customer.business.Bookmark;

public class BookmarkBusiness : IBookmarkBusiness
{
    private readonly IBookmarkRepository _repos;
    private readonly StringCipher _stringCipher;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApiResponseMessageService _apiResponseMessageService;
    private readonly IRecommendationRepository _recommendationRepository;

    public BookmarkBusiness(IBookmarkRepository repos, StringCipher stringCipher, IHttpContextAccessor httpContextAccessor, ApiResponseMessageService apiResponseMessageService, IRecommendationRepository recommendationRepository)
    {
        _repos = repos;
        _stringCipher = stringCipher;
        _httpContextAccessor = httpContextAccessor;
        _apiResponseMessageService = apiResponseMessageService;
        _recommendationRepository = recommendationRepository;
    }
    #region GET BOOKMARK CLUB AND HOST LIST
    public async Task<(HttpStatusCode statusCode, object response)> GetBookmarkHostAndClubList(AmazonSettingsCommon awsSettingCommon)
    {
        var response = new CommonApiResponse()
        {
            errors = new List<Errors>()
        };
        string userId = _httpContextAccessor.HttpContext?.User?.Claims.First(x => x.Type == UserClaimsName.UserId).Value;

        if (!string.IsNullOrEmpty(userId))
            userId = _stringCipher.Decrypt(userId);

        BookmarkTabList common = new BookmarkTabList();
        var timeZoneOffsetValue = _httpContextAccessor.HttpContext?.Items["TimezoneOffset"]?.ToString() ?? string.Empty;
        common.BookmarkedClubs = await GetBookmarkedClubList(userId, timeZoneOffsetValue);
        common.BookmarkedHosts = await GetBookmarkedHostList(userId);
        if (common.BookmarkedClubs.Count != 0 || common.BookmarkedHosts.Count != 0)
        {
            var clubMappedList = common.BookmarkedClubs.MapObjects<BookmarkedClubModel>();
            var hostMappedList = common.BookmarkedHosts.MapObjects<BookmarkedHostModel>();
            clubMappedList = common.BookmarkedClubs.Select(clubItem => new BookmarkedClubModel
            {
                clubId = !string.IsNullOrEmpty(clubItem.clubId) ? _stringCipher.Encrypt(clubItem.clubId) : string.Empty,
                locationId = !string.IsNullOrEmpty(clubItem.locationId) ? _stringCipher.Encrypt(clubItem.locationId) : string.Empty,
                clubName = clubItem.clubName,
                clubNameJapanese = clubItem.clubNameJapanese,
                clubClosingTime = clubItem.clubClosingTime,
                clubOpeningTime = clubItem.clubOpeningTime,
                clubCode = clubItem.clubCode,
                clubTimePeriod = clubItem.clubTimePeriod,
                fullName = clubItem.fullName,
                groupName = clubItem.groupName,
                isBookmarked = clubItem.isBookmarked,
                clubDescription = clubItem.clubDescription,
                locationURL = clubItem.locationURL,
                tag1 = clubItem.tag1,
                tag2 = !string.IsNullOrEmpty(clubItem.tag2) ? "ランク" + "" + clubItem.tag2 : "",
                tag3 = clubItem.tag3,
                tag4 = clubItem.tag4,
                tag5 = clubItem.tag5,
                clubCategory = clubItem.clubCategory,
                landline = clubItem.landline,
                averageRating = clubItem.averageRating,
                totalComment = clubItem.totalComment,
                todaysClubSchedule = clubItem.todaysClubSchedule,
                clubHostImages = clubItem.clubHostImages
                                            .Select(item => new HostImageForClubCard
                                            {
                                                hostId = _stringCipher.Encrypt(item.hostId),
                                                imagePathResp = new ImageResponse
                                                {
                                                    alt = "host_image",
                                                    description = "host image in club card",
                                                    path = ImageHandler.ProcessedImage(awsSettingCommon, item.imagePath)
                                                }
                                            })
                                            .ToList(),
                clubBookmarkedClublogo = new ImageResponse
                {
                    alt = clubItem.clubName,
                    description = "club logo image",
                    path = ImageHandler.ProcessedImage(awsSettingCommon, clubItem.clubLogo)
                },
                clubBookmarkedClubCoverImage = new ImageResponse
                {
                    alt = clubItem.clubName,
                    description = "club cover image",
                    path = ImageHandler.ProcessedImage(awsSettingCommon, clubItem.clubCoverPhoto)
                },
                totalClubPlanCount = clubItem.totalClubPlanCount
            }).ToList();

            var tasks = clubMappedList
               .Zip(common.BookmarkedClubs, (encrypted, raw) => (encrypted, raw))
               .Select(async pair =>
               {
                   var clubPlanResponse = await _recommendationRepository.GetRecommendedClubPlan(pair.raw.clubId);

                   var plan = clubPlanResponse.MapObject<RecommendedClubPlanModel>();
                   if (plan != null && !string.IsNullOrWhiteSpace(plan.planId))
                   {
                       plan.planId = _stringCipher.Encrypt(plan.planId);
                   }

                   pair.encrypted.plans = plan;
               });

            await Task.WhenAll(tasks);

            hostMappedList = common.BookmarkedHosts.Select(hostItem => new BookmarkedHostModel
            {
                clubId = !string.IsNullOrEmpty(hostItem.clubId) ? _stringCipher.Encrypt(hostItem.clubId) : string.Empty,
                hostId = !string.IsNullOrEmpty(hostItem.hostId) ? _stringCipher.Encrypt(hostItem.hostId) : string.Empty,
                hostName = hostItem.hostName,
                hostNameJapanese = hostItem.hostNameJapanese,
                hostCode = hostItem.hostCode,
                clubCode = hostItem.clubCode,
                clubName = hostItem.clubName,
                clubNameJap = hostItem.clubNameJap,
                address = hostItem.address,
                bloodType = hostItem.bloodType,
                constellationGroup = hostItem.constellationGroup,
                dob = hostItem.dob,
                formattedDOB = hostItem.formattedDOB,
                height = hostItem.height,
                // liquorStrength = hostItem.liquorStrength,
                locationId = !string.IsNullOrEmpty(hostItem.locationId) ? _stringCipher.Encrypt(hostItem.locationId) : string.Empty,
                locationURL = hostItem.locationURL,
                loveCount = hostItem.loveCount,
                previousOccupation = hostItem.previousOccupation,
                rank = hostItem.rank,
                hostBookmarkedClubLogo = new ImageResponse
                {
                    alt = hostItem.clubName,
                    description = "club logo",
                    path = ImageHandler.ProcessedImage(awsSettingCommon, hostItem.clubLogo)
                },
                hostBookmerkedHostImage = new ImageResponse
                {
                    alt = hostItem.clubName,
                    description = "club cover image",
                    path = ImageHandler.ProcessedImage(awsSettingCommon, hostItem.hostImage)
                },
                mbti = hostItem.mbti,
                hostReviews = hostItem.hostReviews
            }).ToList();
            response.code = ResponseCodes.Code200_Success;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched);
            response.data = new
            {
                clubs = clubMappedList,
                hosts = hostMappedList
            };
            return (HttpStatusCode.OK, response);
        }
        else
        {
            response.code = ResponseCodes.Code200_Success;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_Recordnotfound);
            response.data = Array.Empty<string>();
            return (HttpStatusCode.OK, response);
        }

    }
    private async Task<IList<BookmarkedClubModel>> GetBookmarkedClubList(string? userId, string timeZoneOffsetValue = "")
    {
        var dbResponse = await _repos.GetBookmarkedClubList(userId, timeZoneOffsetValue);
        return dbResponse;
    }
    private async Task<IList<BookmarkedHostModel>> GetBookmarkedHostList(string? userId)
    {
        var dbResponse = await _repos.GetBookmarkedHostList(userId);
        return dbResponse;
    }
    #endregion

    #region MANAGE BOOKMARK CLUB AND HOST
    public async Task<(HttpStatusCode, object)> ManageBookmark(string agentId = "", string hostId = "", string type = "")
    {
        var response = new CommonApiResponse()
        {
            errors = new List<Errors>()
        };
        string userId = _httpContextAccessor.HttpContext?.User?.Claims.First(x => x.Type == UserClaimsName.UserId).Value;
        var dbRequest = new BookmarkCommonModel();
        dbRequest.agentId = string.IsNullOrEmpty(agentId) ? string.Empty : _stringCipher.Decrypt(agentId);
        dbRequest.hostId = string.IsNullOrEmpty(hostId) ? string.Empty : _stringCipher.Decrypt(hostId);
        dbRequest.type = string.IsNullOrEmpty(type) ? string.Empty : type;
        dbRequest.userId = string.IsNullOrEmpty(userId) ? string.Empty : _stringCipher.Decrypt(userId);

        var dbResponse = await _repos.ManageBookmark(dbRequest);
        if (dbResponse.code == "1")
        {
            response.code = ResponseCodes.Code400_BadRequest;
            response.message = ResponseMessages.Msg400_BadRequest;
            response.data = Array.Empty<string>();
            response.errors = new List<Errors>
            {
                new Errors{errorCode=dbResponse.message,errorMessage=await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse.message)   }
            };
            return (HttpStatusCode.BadRequest, response);
        }
        response.code = ResponseCodes.Code200_Success;
        response.message = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse.message);
        return (HttpStatusCode.OK, response);
    }
    #endregion
}
