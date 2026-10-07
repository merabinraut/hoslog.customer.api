using hoslog.customer.common;
using hoslog.customer.common.Cache;
using hoslog.customer.common.Models.Notice;
using hoslog.customer.common.Service;
using hoslog.customer.common.Utilities;
using hoslog.customer.domain.Notice;
using hoslog.customer.shared;
using hoslog.customer.shared.Notice;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace hoslog.customer.business.Notice
{
    public class NoticeBusiness : INoticeBusiness
    {
        private readonly INoticeRepository _noticeRepository;
        private readonly StringCipher _stringCipher;
        private readonly CommonResponseHandler _commonResponseHandler;
        private readonly ApiResponseMessageService _apiResponseMessageService;
        private readonly RedisCacheSetting _cacheSettings;
        private readonly IResponseCacheService _cacheService;
        public NoticeBusiness(INoticeRepository noticeRepository,
            StringCipher stringCipher,
            CommonResponseHandler commonResponseHandler,
            ApiResponseMessageService apiResponseMessageService,
            RedisCacheSetting cacheSettings,
            IServiceProvider serviceProvider)
        {
            _noticeRepository = noticeRepository;
            _stringCipher = stringCipher;
            _commonResponseHandler = commonResponseHandler;
            _apiResponseMessageService = apiResponseMessageService;
            _cacheSettings = cacheSettings;
            _cacheService = cacheSettings.isEnabled
                   ? serviceProvider.GetService<IResponseCacheService>()
                   : null;
        }

        public async Task<(HttpStatusCode, object)> GetClubNoticeAsync(int pageNo, string clubId, string clubCode)
        {
            var decryptedClubId = _stringCipher.Decrypt(clubId);
            if (string.IsNullOrWhiteSpace(decryptedClubId) && string.IsNullOrWhiteSpace(clubCode))
            {
                return await _commonResponseHandler.HandleBadRequestResponse<CommonApiResponse>(
                    responseDetailMessage: "One or more field validations failed",
                    fieldErrors: new()
                    {
                        new()
                        {
                            errorCode = ResponseCodes.CodeCUE104_ClubId_or_Clubcode_is_required,
                            errorMessage = await _apiResponseMessageService
                                .GetMessageByCodeAsync(ResponseCodes.CodeCUE104_ClubId_or_Clubcode_is_required)
                        }
                    });
            }

            string cacheKey = _cacheSettings.isEnabled ?
                _cacheService.GenerateCacheKey(
                $"GetClubNoticeAsync:pageNo={pageNo}:clubId={clubId ?? "null"}:clubCode={clubCode ?? "null"}")
                : null;

            if (_cacheSettings.isEnabled && _cacheService != null)
            {
                var (cachedStatus, cachedValue) =
                    await _cacheService.GetCachedTupleResponseAsync<CommonApiResponse>(cacheKey);

                if (cachedStatus == HttpStatusCode.OK && cachedValue != null)
                    return (cachedStatus, cachedValue);
            }

            var request = new NoticeModel
            {
                pageNo = pageNo,
                clubId = decryptedClubId,
                clubCode = clubCode
            };

            var response = await _noticeRepository.GetClubNoticeAsync(request);
            var first = response?.FirstOrDefault();

            var resp = new CommonApiResponse();

            if (first?.code == "0")
            {
                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService
                    .GetMessageByCodeAsync(ResponseCodes.CodeC701_RecordFetched);
                resp.data = response.MapObjects<NoticeDetailResponse>();

                if (_cacheSettings.isEnabled && _cacheService != null)
                {
                    await _cacheService.CacheTupleResponseAsync(
                        cacheKey,
                        HttpStatusCode.OK,
                        resp,
                        TimeSpan.FromMinutes(_cacheSettings.minimumIntervalInMinutes));
                }

                return (HttpStatusCode.OK, resp);
            }

            if (first?.code == "1")
            {
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = ResponseMessages.Msg400_BadRequest;
                resp.data = Array.Empty<string>();
                resp.errors = new List<Errors>
                {
                    new()
                    {
                        errorCode = resp.message,
                        errorMessage = await _apiResponseMessageService
                            .GetMessageByCodeAsync(resp.message)
                    }
                };

                return (HttpStatusCode.BadRequest, resp);
            }

            resp.code = ResponseCodes.Code200_Success;
            resp.message = await _apiResponseMessageService
                .GetMessageByCodeAsync(ResponseCodes.CodeC702_Recordnotfound);
            resp.data = Array.Empty<string>();

            return (HttpStatusCode.BadRequest, resp);
        }
    }
}
