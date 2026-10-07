using hoslog.customer.business.CommonHandler;
using hoslog.customer.common.Cache;
using hoslog.customer.common.Service;
using hoslog.customer.common.Utilities;
using hoslog.customer.domain.Banner;
using hoslog.customer.shared;
using hoslog.customer.shared.Banner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace hoslog.customer.business.Banner
{
    public class BannerBusiness : IBannerBusiness
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly StringCipher _stringCipher;
        private readonly RedisCacheSetting _cacheSettings;
        private readonly IResponseCacheService _cacheService;
        private readonly ApiResponseMessageService _apiResponseMessageService;
        private readonly IConfiguration _configuration;
        public BannerBusiness(
            IBannerRepository bannerRepository,
            StringCipher stringCipher,
            RedisCacheSetting cacheSettings,
            ApiResponseMessageService apiResponseMessageService,
            IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            _bannerRepository = bannerRepository;
            _stringCipher = stringCipher;
            _cacheSettings = cacheSettings;
            _apiResponseMessageService = apiResponseMessageService;
            _configuration = configuration;
            _cacheService = cacheSettings.isEnabled
                ? serviceProvider.GetService<IResponseCacheService>()
                : null;
        }

        public async Task<(HttpStatusCode, object)> GetBannerList()
        {
            string cacheKey = _cacheSettings.isEnabled ? _cacheService.GenerateCacheKey(nameof(GetBannerList)) : null;

            if (_cacheSettings.isEnabled && _cacheService != null)
            {
                var (cachedStatus, cachedValue) =
                    await _cacheService.GetCachedTupleResponseAsync<CommonApiResponse>(cacheKey);

                if (cachedStatus == HttpStatusCode.OK && cachedValue != null)
                    return (cachedStatus, cachedValue);
            }

            var repoResp = await _bannerRepository.GetBannerList();
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

            var bannerResponseList = repoResp.Select(banner => new BannerResponse
            {
                id = _stringCipher.Encrypt(banner.BannerId),
                title = banner.bannerTitle,
                image = new ImageResponse
                {
                    alt = banner.bannerTitle,
                    path = ImageHandler.ProcessedImage(amazonSettingsCommon, banner.bannerImage),
                    description = null
                }
            }).ToList();

            var response = new CommonApiResponse
            {
                data = bannerResponseList,
                code = ResponseCodes.Code200_Success,
                message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched),
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
