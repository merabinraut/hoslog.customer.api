using System.Net;
using hoslog.customer.business.CommonHandler;
using hoslog.customer.common;
using hoslog.customer.common.Cache;
using hoslog.customer.common.Models.Group;
using hoslog.customer.common.Service;
using hoslog.customer.common.Utilities;
using hoslog.customer.domain.Group;
using hoslog.customer.shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace hoslog.customer.business.Group;

public class GroupBusiness : IGroupBusiness
{
    private readonly IGroupRepository _repo;
    private readonly IConfiguration _configuration;
    private readonly StringCipher _stringCipher;
    private readonly AmazonSettingsCommon _amazonSettingsCommon;
    private readonly ApiResponseMessageService _apiResponseMessageService;
    private readonly RedisCacheSetting _cacheSettings;
    private readonly IResponseCacheService _cacheService;
    public GroupBusiness(IGroupRepository repository,
        IConfiguration configuration,
        StringCipher stringCipher,
        RedisCacheSetting cacheSettings,
        ApiResponseMessageService apiResponseMessageService,
        IServiceProvider serviceProvider)
    {
        _repo = repository;
        _configuration = configuration;
        _stringCipher = stringCipher;
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
        _apiResponseMessageService = apiResponseMessageService;
        _cacheService = cacheSettings.isEnabled
               ? serviceProvider.GetService<IResponseCacheService>()
               : null;
    }

    public async Task<(HttpStatusCode, object)> GetGroupDetail(GroupRequestModel requestModel)
    {
        string cacheKey = _cacheSettings.isEnabled ? _cacheService.GenerateCacheKey(
            $"GetGroupDetail:groupId={requestModel.groupId ?? "null"}:clubId={requestModel.clubId ?? "null"}"
        ) : null;

        if (_cacheSettings.isEnabled && _cacheService != null)
        {
            var (cachedStatus, cachedValue) =
                await _cacheService.GetCachedTupleResponseAsync<CommonApiResponse>(cacheKey);

            if (cachedStatus == HttpStatusCode.OK && cachedValue != null)
                return (cachedStatus, cachedValue);
        }

        requestModel.groupId = _stringCipher.Decrypt(requestModel.groupId);
        requestModel.clubId = _stringCipher.Decrypt(requestModel.clubId);

        var groupDetail = await GetGroupDetailViaId(requestModel.groupId, requestModel.clubId);
        var subGroupAndClubList = new List<GroupedSubGroupClubModel>();

        if (!string.IsNullOrEmpty(requestModel.groupId))
            subGroupAndClubList = await GetSubGroupAndClubListViaGroupId(requestModel.groupId);

        if (!string.IsNullOrEmpty(requestModel.clubId))
            subGroupAndClubList = await GetSubGroupAndClubListViaClubId(requestModel.clubId);

        var groupGallery = string.IsNullOrEmpty(requestModel.groupId)
                ? new List<GroupGalleryModel>()
                : (await GroupGalleryList(requestModel.groupId)).ToList();

        var response = new CommonApiResponse
        {
            code = ResponseCodes.Code200_Success,
            message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched),
            errors = new List<Errors>(),
            data = new
            {
                groupDetail = groupDetail.FirstOrDefault(),
                groupGallery,
                subGroupAndClubList
            }
        };

        if (_cacheSettings.isEnabled)
        {
            await _cacheService.CacheTupleResponseAsync(
                cacheKey,
                HttpStatusCode.OK,
                response,
                TimeSpan.FromMinutes(_cacheSettings.maximumIntervalInMinutes));
        }

        return (HttpStatusCode.OK, response);
    }

    private async Task<List<GroupedSubGroupClubModel>> GetSubGroupAndClubListViaGroupId(string groupId)
    {
        var dbResponse = await _repo.GetSubGroupAndClubListViaGroupId(groupId);
        var respMapped = dbResponse.MapObjects<AssignedGroupClubModel>();
        if (respMapped != null && respMapped.Count > 0)
        {
            respMapped = dbResponse.Select(item => new AssignedGroupClubModel
            {
                clubId = item.clubId != null ? _stringCipher.Encrypt(item.clubId) : string.Empty,
                locationId = string.IsNullOrEmpty(item.locationId) ? string.Empty : _stringCipher.Encrypt(item.locationId),
                clubCode = item.clubCode,
                subGroupId = item.subGroupId != null ? _stringCipher.Encrypt(item.subGroupId) : string.Empty,
                subGroupName = item.subGroupName,
                clubName = item.clubName,
                clubNameJapanese = item.clubNameJapanese,
                description = item.description,
                imageResponse = new ImageResponse()
                {
                    alt = item.clubName,
                    description = "club_logo",
                    path = item.clubLogo != null ? ImageHandler.ProcessedImage(_amazonSettingsCommon, item.clubLogo) : string.Empty
                },
                resClubCoverPhoto = new ImageResponse
                {
                    alt = string.IsNullOrWhiteSpace(item.clubCoverPhoto) ? null : item.clubName,
                    description = string.IsNullOrWhiteSpace(item.clubCoverPhoto) ? null : "club cover image",
                    path = string.IsNullOrWhiteSpace(item.clubCoverPhoto) ? null : ImageHandler.ProcessedImage(_amazonSettingsCommon, item.clubCoverPhoto),
                },

                //hostImagesList = new List<ImageResponse>{
                //    new ImageResponse()
                //    {
                //        alt = item.clubName,
                //        description = "host_image",
                //        path = item.hostImages != null && !string.IsNullOrEmpty(item.hostImages)
                //            ? ImageHandler.ProcessedImage(_amazonSettingsCommon, item.hostImages)
                //            : string.Empty
                //    }
                hostImagesList = new List<ImageResponse> {
                        new ImageResponse() {
                             alt = item.clubName,
                             description = "host_image",
                             path = item.hostImages != null && !string.IsNullOrEmpty(item.hostImages)
            ? string.Join(",", item.hostImages.Split(',').Select(image => ImageHandler.ProcessedImage(_amazonSettingsCommon, image)))
            : string.Empty
                }
            }
            }).ToList();
            var groupedData = respMapped
           .GroupBy(x => new { x.subGroupId, x.subGroupName })
           .Select(group => new GroupedSubGroupClubModel
           {
               subGroupId = group.Key.subGroupId,
               subGroupName = group.Key.subGroupName,
               clubs = group.ToList()
           })
           .ToList();
            return groupedData;
        }
        return new List<GroupedSubGroupClubModel>();
    }
    private async Task<List<GroupedSubGroupClubModel>> GetSubGroupAndClubListViaClubId(string clubId)
    {
        var dbResponse = await _repo.GetSubGroupAndClubListViaClubId(clubId);
        var mappedResponse = dbResponse.MapObjects<AssignedGroupClubModel>();
        if (mappedResponse != null && mappedResponse.Count > 0)
        {
            mappedResponse = dbResponse.Select(item => new AssignedGroupClubModel
            {
                clubId = !string.IsNullOrEmpty(item.clubId) ? _stringCipher.Encrypt(item.clubId) : string.Empty,
                locationId = !string.IsNullOrEmpty(item.locationId) ? _stringCipher.Encrypt(item.locationId) : string.Empty,
                clubCode = item.clubCode,
                subGroupId = item.subGroupId != null ? _stringCipher.Encrypt(item.subGroupId) : string.Empty,
                subGroupName = item.subGroupName,
                clubName = item.clubName,
                clubNameJapanese = item.clubNameJapanese,
                description = item.description,
                imageResponse = new ImageResponse()
                {
                    alt = item.clubName,
                    description = "club_logo",
                    path = item.clubLogo != null ? ImageHandler.ProcessedImage(_amazonSettingsCommon, item.clubLogo) : string.Empty
                },
                resClubCoverPhoto = new ImageResponse
                {
                    alt = string.IsNullOrWhiteSpace(item.clubCoverPhoto) ? null : item.clubName,
                    description = string.IsNullOrWhiteSpace(item.clubCoverPhoto) ? null : "club cover image",
                    path = string.IsNullOrWhiteSpace(item.clubCoverPhoto) ? null : ImageHandler.ProcessedImage(_amazonSettingsCommon, item.clubCoverPhoto),
                }

            }).ToList();
            var groupedData = mappedResponse
          .GroupBy(x => new { x.subGroupId, x.subGroupName })
          .Select(group => new GroupedSubGroupClubModel
          {
              subGroupId = group.Key.subGroupId,
              subGroupName = group.Key.subGroupName,
              clubs = group.ToList()
          })
          .ToList();
            return groupedData;
        }
        return new List<GroupedSubGroupClubModel>();
    }


    private async Task<IList<GroupDetailModel>> GetGroupDetailViaId(string groupId, string clubId)
    {
        var dbResponse = await _repo.GetGroupDetailViaId(groupId, clubId);
        if (dbResponse != null && dbResponse.Count > 0)
        {
            var respMapped = dbResponse.Select(item => new GroupDetailModel
            {
                groupId = item.groupId != null ? _stringCipher.Encrypt(item.groupId) : string.Empty,
                groupName = item.groupName,
                groupNameKatakana = item.groupNameKatakana,
                clubCode = item.clubCode,
                imageResponse = new ImageResponse()
                {
                    alt = item.groupName,
                    description = "group_image",
                    path = item.image != null ? ImageHandler.ProcessedImage(_amazonSettingsCommon, item.image) : string.Empty
                }
            }).ToList();
            return respMapped;
        }
        else
        {
            return Array.Empty<GroupDetailModel>();
        }

    }
    private async Task<IList<GroupGalleryModel>> GroupGalleryList(string groupId)
    {
        var dbResponse = await _repo.GroupGalleryList(groupId);
        if (dbResponse != null && dbResponse.Count > 0)
        {
            var respMapped = dbResponse.Select(item => new GroupGalleryModel
            {
                imageResponse = new ImageResponse()
                {
                    alt = "group_image",
                    description = "group_image",
                    path = item.image != null ? ImageHandler.ProcessedImage(_amazonSettingsCommon, item.image) : string.Empty
                }
            }).ToList();
            return respMapped;
        }
        else
        {
            return Array.Empty<GroupGalleryModel>();
        }
    }

    public async Task<(HttpStatusCode, object)> GetGroupList(string freeWord = "", string vowel = "", int pageNo = 1)
    {
        string cacheKey = _cacheSettings.isEnabled ? _cacheService.GenerateCacheKey(
            $"GetGroupList:freeWord={freeWord ?? "null"}:vowel={vowel ?? "null"}:pageNo={pageNo}"
        ) : null;

        if (_cacheSettings.isEnabled && _cacheService != null)
        {
            var (cachedStatus, cachedValue) =
                await _cacheService.GetCachedTupleResponseAsync<CommonApiResponse>(cacheKey);

            if (cachedStatus == HttpStatusCode.OK && cachedValue != null)
                return (cachedStatus, cachedValue);
        }

        var response = new CommonApiResponse
        {
            code = ResponseCodes.Code200_Success,
            errors = new List<Errors>()
        };

        var dbResponse = await _repo.GetGroupList(freeWord, vowel, pageNo);

        if (dbResponse == null || !dbResponse.Any())
        {
            response.message = await _apiResponseMessageService
                .GetMessageByCodeAsync(ResponseCodes.CodeC702_Recordnotfound);

            return (HttpStatusCode.OK, response);
        }

        var firstRecord = dbResponse.First();

        var mappedGroupList = dbResponse.Select(item => new GroupInfoModel
        {
            groupId = item.groupId != null ? _stringCipher.Encrypt(item.groupId) : string.Empty,
            groupName = item.groupName,
            groupNameKatakana = item.groupNameKatakana,
            imageResponse = new ImageResponse
            {
                alt = item.groupName,
                description = "group_image",
                path = item.image != null
                    ? ImageHandler.ProcessedImage(_amazonSettingsCommon, item.image)
                    : string.Empty
            }
        }).ToList();

        response.data = new GroupListResponse
        {
            pagination = firstRecord.MapObject<PaginationResponse>(),
            groupList = mappedGroupList
        };

        response.message = await _apiResponseMessageService
            .GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched);

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
}
