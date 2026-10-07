using System.Net;

namespace hoslog.customer.common.Cache
{
    public interface IResponseCacheService
    {
        void SetCache(string cacheKey, string cacheValue, TimeSpan timeToLive);
        Task SetCacheAsync(string cacheKey, string cacheValue, TimeSpan timeToLive);
        void CacheResponse<T>(string cacheKey, T cacheValue, TimeSpan timeToLive);
        Task CacheResponseAsync<T>(string cacheKey, T cacheValue, TimeSpan timeToLive);
        string GetCachedResponse(string cacheKey);
        Task<string> GetCachedResponseAsync(string cacheKey);
        void CacheTupleResponse<T>(string cacheKey, HttpStatusCode statusCode, T cacheValue, TimeSpan timeToLive);
        Task CacheTupleResponseAsync<T>(string cacheKey, HttpStatusCode statusCode, T cacheValue, TimeSpan timeToLive);
        (HttpStatusCode, T) GetCachedTupleResponse<T>(string cacheKey);
        Task<(HttpStatusCode, T)> GetCachedTupleResponseAsync<T>(string cacheKey);
        string GenerateCacheKey(string methodName, params object[] parameters);
        void ClearAllCache();
        Task ClearAllCacheAsync();
    }
}
