using hoslog.customer.business.CommonHandler;
using hoslog.customer.common.Cache;
using hoslog.customer.common.Service;
using hoslog.customer.common.Utilities;
using hoslog.customer.domain.Advertisement;
using hoslog.customer.shared;
using hoslog.customer.shared.Advertisement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace hoslog.customer.business.Advertisement
{
    public class AdvertisementBusiness : IAdvertisementBusiness
    {
        private readonly IAdvertisementRepository _adsRepository;
        private readonly StringCipher _stringCipher;
        private readonly ApiResponseMessageService _apiResponseMessageService;
        private readonly RedisCacheSetting _cacheSettings;
        private readonly IResponseCacheService _cacheService;
        private readonly IConfiguration _configuration;
        public AdvertisementBusiness(IAdvertisementRepository adsRepository,
            StringCipher stringCipher,
            ApiResponseMessageService apiResponseMessageService,
            RedisCacheSetting cacheSettings,
            IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            _adsRepository = adsRepository;
            _stringCipher = stringCipher;
            _apiResponseMessageService = apiResponseMessageService;
            _cacheSettings = cacheSettings;
            _configuration = configuration;
            _cacheService = cacheSettings.isEnabled
                ? serviceProvider.GetService<IResponseCacheService>()
                : null;
        }

        public async Task<(HttpStatusCode, object)> GetAdvertisementList()
        {
            string cacheKey = _cacheSettings.isEnabled ? _cacheService.GenerateCacheKey(nameof(GetAdvertisementList)) : null;

            if (_cacheSettings.isEnabled && _cacheService != null)
            {
                var (cachedStatus, cachedValue) =
                    await _cacheService.GetCachedTupleResponseAsync<CommonApiResponse>(cacheKey);

                if (cachedStatus == HttpStatusCode.OK && cachedValue != null)
                    return (cachedStatus, cachedValue);
            }

            var repoResp = await _adsRepository.GetAdvertisementList();

            if (repoResp == null || repoResp.Count == 0)
            {
                var emptyResponse = new CommonApiResponse
                {
                    code = ResponseCodes.Code200_Success,
                    message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_Recordnotfound),
                    data = Array.Empty<string>()
                };
                return (HttpStatusCode.OK, emptyResponse);
            }

            var amazonSettingsCommon = new AmazonSettingsCommon
            {
                BucketName = _configuration["BucketName"],
                BaseUrl = _configuration["BaseUrl"],
                ConfigAccess = _configuration["ConfigAccess"],
                ConfigSecret = _configuration["ConfigSecret"],
                ImageVirtualPath = _configuration["ImageVirtualPath"],
                PreviousImageUrl = _configuration["PreviousImageUrl"]
            };

            var adsList = repoResp.Select(ads => new AdvertisementResponse
            {
                id = _stringCipher.Encrypt(ads.sno),
                title = ads.advertisementTitle,
                image = new ImageResponse
                {
                    alt = ads.advertisementTitle,
                    path = ImageHandler.ProcessedImage(amazonSettingsCommon, ads.advertisementImage),
                    description = ads.advertisementDescription
                },
                link = ads.link,
                displayOrder = ads.displayOrder
            }).ToList();

            var response = new CommonApiResponse
            {
                code = ResponseCodes.Code200_Success,
                message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched),
                data = adsList
            };

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
}
