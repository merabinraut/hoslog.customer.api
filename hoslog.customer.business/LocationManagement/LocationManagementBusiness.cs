using hoslog.customer.business.CommonHandler;
using hoslog.customer.domain.LocationManagement;
using hoslog.customer.shared;
using System.Net;
using hoslog.customer.shared.LocationManagement;
using hoslog.customer.common.Utilities;
using hoslog.customer.common.Service;
using Microsoft.Extensions.Configuration;
using hoslog.customer.common.Cache;
using Microsoft.Extensions.DependencyInjection;

namespace hoslog.customer.business.LocationManagement
{
    public class LocationManagementBusiness : ILocationManagementBusiness
    {
        private readonly ILocationManagementRepository _locationRepository;
        private readonly CommonResponseHandler _commonResponseHandler;
        private readonly StringCipher _stringCipher;
        private readonly ApiResponseMessageService _apiResponseMessageService;
        private readonly IConfiguration _configuration;
        private readonly RedisCacheSetting _cacheSettings;
        private readonly IResponseCacheService _cacheService;
        private readonly AmazonSettingsCommon _amazonSettings;
        public LocationManagementBusiness(ILocationManagementRepository locationRepository,
            StringCipher stringCipher,
            CommonResponseHandler commonResponseHandler,
            ApiResponseMessageService apiResponseMessageService,
            IConfiguration configuration,
            RedisCacheSetting cacheSettings,
            IServiceProvider serviceProvider)
        {
            _locationRepository = locationRepository;
            _commonResponseHandler = commonResponseHandler;
            _stringCipher = stringCipher;
            _apiResponseMessageService = apiResponseMessageService;
            _cacheSettings = cacheSettings;
            _configuration = configuration;
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

        public async Task<(HttpStatusCode, object)> GetLocationList()
        {
            var resp = new CommonApiResponse
            {
                errors = new List<Errors>()
            };

            string cacheKey = _cacheSettings.isEnabled ? _cacheService.GenerateCacheKey(nameof(GetLocationList)) : null;

            if (_cacheSettings.isEnabled && _cacheService != null)
            {
                var (cachedStatus, cachedValue) =
                    await _cacheService.GetCachedTupleResponseAsync<CommonApiResponse>(cacheKey);

                if (cachedStatus == HttpStatusCode.OK && cachedValue != null)
                    return (cachedStatus, cachedValue);
            }

            var repoResp = await _locationRepository.GetLocationList();

            if (repoResp == null || repoResp.Count == 0)
            {
                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService
                    .GetMessageByCodeAsync(ResponseCodes.CodeC702_Recordnotfound);
                resp.data = Array.Empty<string>();
                return (HttpStatusCode.OK, resp);
            }

            var locList = repoResp.Select(locs =>
            {
                _ = int.TryParse(locs.clubCount, out var clubCount);
                _ = int.TryParse(locs.hostCount, out var hostCount);

                var isActive = string.Equals(
                    locs.isActive,
                    "A",
                    StringComparison.OrdinalIgnoreCase);

                var imagePath = !string.IsNullOrEmpty(locs.imagePath)
                    ? ImageHandler.ProcessedImage(_amazonSettings, locs.imagePath)
                    : string.Empty;

                return new LocationCommonResponse
                {
                    id = !string.IsNullOrEmpty(locs.id)
                        ? _stringCipher.Encrypt(locs.id)
                        : string.Empty,

                    subLocationName = locs.subLocationName,
                    subLocationNameEng = locs.subLocationNameEng,
                    prefecture = locs.prefecture,
                    locationName = locs.locationName,
                    isActive = isActive,
                    clubCount = clubCount,
                    hostCount = hostCount,
                    image = new ImageResponse
                    {
                        alt = locs.locationName,
                        path = imagePath,
                        description = "location name"
                    },
                    location = new LocationResponse
                    {
                        url = locs.locationURL,
                        longitude = locs.longitude,
                        latitude = locs.latitude
                    }
                };
            }).ToList();

            resp.data = locList;
            resp.code = ResponseCodes.Code200_Success;
            resp.message = await _apiResponseMessageService
                .GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched);

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
    }
}
