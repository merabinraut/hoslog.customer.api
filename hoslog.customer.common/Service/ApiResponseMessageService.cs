using Dapper;
using hoslog.customer.common.Cache;
using hoslog.customer.common.Utilities;
using hoslog.customer.shared.ApiMessageManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace hoslog.customer.common.Service
{
    public class ApiResponseMessageService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly RedisCacheSetting _cacheSettings;
        private readonly IResponseCacheService? _cacheService;
        private readonly SqlHandler _sqlHandler;
        public ApiResponseMessageService(IHttpContextAccessor httpContextAccessor,
            RedisCacheSetting cacheSettings,
            SqlHandler sqlHandler,
            IServiceProvider serviceProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _cacheSettings = cacheSettings;
            _sqlHandler = sqlHandler;
            _cacheService = cacheSettings.isEnabled
                ? serviceProvider.GetService<IResponseCacheService>()
                : null;
        }

        public async Task<string> GetMessageByCodeAsync(string messageCode)
        {
            if (string.IsNullOrWhiteSpace(messageCode))
                return string.Empty;

            var language = GetLanguage();
            var columnName = GetMessageColumnByLanguage(language);


            string? cacheKey = _cacheSettings.isEnabled
                ? $"apiMessage:{messageCode}:{language}"
                : null;

            if (cacheKey is not null && _cacheService is not null)
            {
                var cached = await _cacheService.GetCachedResponseAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                    return cached;
            }


            const string proc = "[dbo].[apiproc_get_api_response_message_via_code]";
            var parameters = new DynamicParameters(new { messageCode });

            var response = await _sqlHandler.ExecuteAsync<ApiMessageCommon>(proc, parameters);
            var message = response?
                .GetType()
                .GetProperty(columnName)?
                .GetValue(response)?
                .ToString() ?? string.Empty;

            if (!string.IsNullOrEmpty(message) && cacheKey is not null && _cacheService is not null)
            {
                await _cacheService.SetCacheAsync(
                    cacheKey,
                    message,
                    TimeSpan.FromMinutes(_cacheSettings.maximumIntervalInMinutes));
            }

            return string.IsNullOrEmpty(message) ? messageCode : message;
        }

        private string GetLanguage()
        {
            var http = _httpContextAccessor.HttpContext;

            string? lang =
                http?.Items["Language"]?.ToString()
                ?? http?.Request.Headers["Accept-Language"].FirstOrDefault()
                ?? http?.Request.Query["lang"].FirstOrDefault()
                ?? "ja";

            return lang.ToLowerInvariant();
        }

        private static string GetMessageColumnByLanguage(string language) =>
            language switch
            {
                "en" or "eng" => "messageEng",
                _ => "message" // Default Japanese
            };
    }
}