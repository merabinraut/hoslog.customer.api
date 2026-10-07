using hoslog.customer.shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;

namespace hoslog.customer.common.Cache
{
    public class CachedAttribute : Attribute, IAsyncActionFilter
    {
        private readonly int _timeToLiveSeconds;
        public CachedAttribute(int timeToLiveSeconds = 30)
        {
            _timeToLiveSeconds = timeToLiveSeconds;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var cacheSettings = context.HttpContext.RequestServices.GetRequiredService<RedisCacheSetting>();
            if (!cacheSettings.isEnabled)
            {
                await next();
                return;
            }
            var cacheService = context.HttpContext.RequestServices.GetRequiredService<IResponseCacheService>();
            var cacheKey = GenerateCacheKeyFromRequest(context.HttpContext.Request);
            //var cachedResponse = await cacheService.GetCachedResponseAsync<CommonApiResponse>(cacheKey);
            //var cachedResponseObject = cachedResponse != null ? JsonSerializer.Serialize(cachedResponse) : string.Empty;

            var cachedResponse = await cacheService.GetCachedResponseAsync(cacheKey);
            var cachedResponseObject = !string.IsNullOrEmpty(cachedResponse) ? JsonSerializer.Serialize(cachedResponse) : string.Empty;

            if (!string.IsNullOrEmpty(cachedResponseObject))
            {
                var contentResult = new ContentResult
                {
                    StatusCode = StatusCodes.Status200OK,
                    Content = cachedResponseObject,
                    ContentType = "application/json"
                };
                context.Result = contentResult;
                return;
            }
            var executedContext = await next();

            if (executedContext.Result is OkObjectResult okObjectResult)
            {
                await cacheService.CacheResponseAsync(cacheKey, okObjectResult.Value, TimeSpan.FromSeconds(_timeToLiveSeconds));
            }
            else if (executedContext.Result is ObjectResult objectResult && objectResult.StatusCode == StatusCodes.Status200OK)
            {
                await cacheService.CacheResponseAsync(cacheKey, objectResult.Value, TimeSpan.FromSeconds(_timeToLiveSeconds));
            }
        }

        private string GenerateCacheKeyFromRequest(HttpRequest request)
        {
            var keyBuilder = new StringBuilder($"{request.Path}");

            foreach (var (key, value) in request.Query.OrderBy(x => x.Key))
                keyBuilder.Append($"|{key}-{value}");

            return keyBuilder.ToString();
        }
    }
}
