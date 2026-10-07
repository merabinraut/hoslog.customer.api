using hoslog.customer.common.Utilities;
using hoslog.customer.domain.CommonAttribute;
using hoslog.customer.shared;
using System.Net;
using hoslog.customer.shared.Common;
using hoslog.customer.common.Service;
using hoslog.customer.common.Cache;
using Microsoft.Extensions.DependencyInjection;

namespace hoslog.customer.business.CommonAttribute
{
    public class CommonAttributeBusiness : ICommonAttributeBusiness
    {
        private readonly ICommonAttributeRepository _commonRepository;
        private readonly CommonAttributeStringCipher _commonAttributeStringCipher;
        private readonly ApiResponseMessageService _apiResponseMessageService;
        private readonly RedisCacheSetting _cacheSettings;
        private readonly IResponseCacheService _cacheService;
        public CommonAttributeBusiness(ICommonAttributeRepository commonRepository,
            CommonAttributeStringCipher commonAttributeStringCipher,
            ApiResponseMessageService apiResponseMessageService,
            RedisCacheSetting cacheSettings,
            IServiceProvider serviceProvider)
        {
            _commonRepository = commonRepository;
            _commonAttributeStringCipher = commonAttributeStringCipher;
            _apiResponseMessageService = apiResponseMessageService;
            _cacheSettings = cacheSettings;
            _cacheService = cacheSettings.isEnabled
              ? serviceProvider.GetService<IResponseCacheService>()
              : null;
        }

        public async Task<(HttpStatusCode, object)> GetCommonAttributeList(string flag)
        {
            string cacheKey = _cacheSettings.isEnabled ? _cacheService?.GenerateCacheKey($"GetCommonAttributeList:flag={flag}") : null;

            if (_cacheSettings.isEnabled && _cacheService != null)
            {
                var (cachedStatus, cachedValue) = await _cacheService.GetCachedTupleResponseAsync<CommonApiResponse>(cacheKey);
                if (cachedStatus == HttpStatusCode.OK && cachedValue != null)
                    return (cachedStatus, cachedValue);
            }

            var response = new CommonApiResponse();
            var repoResp = await _commonRepository.GetCommonAttributeList(flag);

            if (repoResp == null || !repoResp.Any())
            {
                response.code = ResponseCodes.Code200_Success;
                response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_Recordnotfound);
                response.data = Array.Empty<string>();
                return (HttpStatusCode.OK, response);
            }

            response.data = repoResp.Select(res => new AttributeCommon
            {
                staticValue = _commonAttributeStringCipher.Encrypt(res.staticValue),
                staticLabel = res.staticLabel,
                staticLabelEng = res.staticLabelEng

            }).ToList();

            response.code = ResponseCodes.Code200_Success;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched);

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

        public async Task ClearAllCacheAsync()
        {
            if (_cacheSettings.isEnabled)
            {
                await _cacheService.ClearAllCacheAsync();
            }
        }

        #region refer
        public async Task<(HttpStatusCode, CommonApiResponse)> UpdateReferralClickCount(string referCode)
        {
            if (string.IsNullOrEmpty(referCode))
            {
                return (HttpStatusCode.BadRequest, new CommonApiResponse
                {
                    code = ResponseCodes.Code400_BadRequest,
                    message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CUEREF100_ReferCodeIsRequired)
                });
            }

            var dbRequest = new ReferralClickCountUpdateRequestCommon
            {
                referCode = referCode
            };

            var dbResponse = await _commonRepository.UpdateReferralClickCount(dbRequest);

            if (dbResponse is null || dbResponse.code != "0")
            {
                var msgCode = string.IsNullOrEmpty(dbResponse?.message)
                  ? ResponseCodes.Code400_BadRequest
                  : dbResponse!.message;

                return (HttpStatusCode.BadRequest, new CommonApiResponse
                {
                    code = ResponseCodes.Code400_BadRequest,
                    message = await _apiResponseMessageService.GetMessageByCodeAsync(msgCode)
                });
            }

            var successMsgCode = string.IsNullOrWhiteSpace(dbResponse.message)
                ? ResponseCodes.Code200_Success
                : dbResponse.message;

            return (HttpStatusCode.OK, new CommonApiResponse
            {
                code = ResponseCodes.Code200_Success,
                message = await _apiResponseMessageService.GetMessageByCodeAsync(successMsgCode)
            });
        }
        #endregion
    }
}