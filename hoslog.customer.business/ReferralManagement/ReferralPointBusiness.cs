using hoslog.customer.common.Cache;
using hoslog.customer.common.Models.Auth;
using hoslog.customer.common.Models.Referral;
using hoslog.customer.common.Service;
using hoslog.customer.common.Utilities;
using hoslog.customer.domain.ReferralManagement;
using hoslog.customer.shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace hoslog.customer.business.ReferralManagement
{
    public class ReferralPointBusiness : IReferralPointBusiness
    {
        private readonly IReferralPointRepository _referralPointInfoRepository;
        private readonly StringCipher _stringCipher;
        private readonly ApiResponseMessageService _apiResponseMessageService;
        private readonly RedisCacheSetting _cacheSettings;
        private readonly IResponseCacheService _cacheService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ReferralPointBusiness(IReferralPointRepository referralPointInfoRepository, IHttpContextAccessor httpContextAccessor,
            StringCipher stringCipher,
            ApiResponseMessageService apiResponseMessageService,
            RedisCacheSetting cacheSettings,
            IServiceProvider serviceProvider)
        {
            _referralPointInfoRepository = referralPointInfoRepository;
            _stringCipher = stringCipher;
            _apiResponseMessageService = apiResponseMessageService;
            _cacheSettings = cacheSettings;
            _httpContextAccessor = httpContextAccessor;
            _cacheService = cacheSettings.isEnabled
                ? serviceProvider.GetService<IResponseCacheService>()
                : null;
        }

        public async Task<(HttpStatusCode, object)> GetReferralPointInfo()
        {
            var response = new CommonApiResponse
            {
                errors = new List<Errors>()
            };

            string cacheKey = (_cacheSettings.isEnabled && _cacheService != null) ? _cacheService.GenerateCacheKey(nameof(GetReferralPointInfo)) : null;

            if (_cacheSettings.isEnabled && _cacheService != null)
            {
                var (cachedStatus, cachedValue) = await _cacheService.GetCachedTupleResponseAsync<CommonApiResponse>(cacheKey);
                if (cachedStatus == HttpStatusCode.OK && cachedValue != null)

                    return (cachedStatus, cachedValue);
            }

            var data = await _referralPointInfoRepository.GetReferralPointInfo();

            if (data == null || data.Count == 0)
            {
                response.code = ResponseCodes.Code200_Success;
                response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_Recordnotfound);
                response.data = Array.Empty<string>();
                return (HttpStatusCode.OK, response);
            }

            var dbResponse = new ReferralPointInfoResponse
            {
                stage1 = data
                .Where(x => x.stageCode == "STAGE_1")
                .Select(x => new ReferralPointInfoModel
                {
                    stageCode = x.stageCode,
                    reservationCount = x.reservationCount,
                    points = x.points
                }).ToList(),

                stage2 = data
                .Where(x => x.stageCode == "STAGE_2")
                .Select(x => new ReferralPointInfoModel
                {
                    stageCode = x.stageCode,
                    reservationCount = x.reservationCount,
                    points = x.points
                }).FirstOrDefault()

            };

            response.code = ResponseCodes.Code200_Success;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched);

            response.data = dbResponse;


            if (_cacheSettings.isEnabled && _cacheService != null)
            {
                await _cacheService.CacheTupleResponseAsync(cacheKey, HttpStatusCode.OK, response, TimeSpan.FromMinutes(_cacheSettings.maximumIntervalInMinutes));
            }

            return (HttpStatusCode.OK, response);
        }

        public async Task<(HttpStatusCode, object)> GetTotalReferralPoint()
        {
            var response = new CommonApiResponse
            {
                errors = new List<Errors>()
            };

            string agentId = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(x => x.Type == UserClaimsName.AgentId)?.Value;

            if (string.IsNullOrEmpty(agentId))
            {
                response.code = ResponseCodes.Code400_BadRequest;
                response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);

                return (HttpStatusCode.BadRequest, response);
            }

            agentId = _stringCipher.Decrypt(agentId);

            if (!long.TryParse(agentId, out long customerId))
            {
                response.code = ResponseCodes.Code400_BadRequest;
                response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);

                return (HttpStatusCode.BadRequest, response);
            }

            var data = await _referralPointInfoRepository.GetTotalReferralPoint(customerId.ToString());

            if (data == null)
            {
                response.code = ResponseCodes.Code200_Success;
                response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_Recordnotfound);
                response.data = Array.Empty<string>();
                return (HttpStatusCode.OK, response);
            }

            var mappedResponse = new ReferralPointTotalModel
            {
                referralPoints = data.referralPoints,
                reviewPoints = data.reviewPoints,
                totalReferralPoints = data.totalReferralPoints
            };

            response.code = ResponseCodes.Code200_Success;  
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched);
            response.data = mappedResponse;
            return (HttpStatusCode.OK, response);

        }

        public async Task<(HttpStatusCode, object)> GetReferralInfoStatus()
        {
            var response = new CommonApiResponse
            {
                errors = new List<Errors>()
            };
            string agentId = _httpContextAccessor.HttpContext?.User?.Claims.FirstOrDefault(x => x.Type == UserClaimsName.AgentId)?.Value;

            if (string.IsNullOrEmpty(agentId))
            {
                response.code = ResponseCodes.Code400_BadRequest;
                response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);

                return (HttpStatusCode.BadRequest, response);
            }

            agentId = _stringCipher.Decrypt(agentId);

            if (!long.TryParse(agentId, out long customerId))
            {
                response.code = ResponseCodes.Code400_BadRequest;
                response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest);

                return (HttpStatusCode.BadRequest, response);
            }

            var data = await _referralPointInfoRepository.GetReferralInfoStatus(customerId.ToString());

            if (data == null)
            {
                response.code = ResponseCodes.Code200_Success;
                response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC702_Recordnotfound);
                response.data = Array.Empty<string>();
                return (HttpStatusCode.OK, response);
            }

            response.code = ResponseCodes.Code200_Success;
            response.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched);
            response.data = data;

            return (HttpStatusCode.OK, response);
        }

    }
}
