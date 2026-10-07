using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Net;
using System.Text.Json;

namespace hoslog.customer.common.Cache
{
    public class ResponseCacheService : IResponseCacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly RedisCacheSetting _cacheSettings;
        public ResponseCacheService(IDistributedCache distributedCache,
            IHttpContextAccessor httpContextAccessor,
            RedisCacheSetting cacheSettings,
            IConnectionMultiplexer redisConnection = null
            )
        {
            _distributedCache = distributedCache;
            _httpContextAccessor = httpContextAccessor;
            _redisConnection = redisConnection;
            _cacheSettings = cacheSettings;
        }

        public void SetCache(string cacheKey, string cacheValue, TimeSpan timeToLive)
        {
            if (string.IsNullOrEmpty(cacheKey) || string.IsNullOrEmpty(cacheValue))
                return;

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = timeToLive
            };
            _distributedCache.SetString(cacheKey, cacheValue, options);
        }

        public async Task SetCacheAsync(string cacheKey, string cacheValue, TimeSpan timeToLive)
        {
            if (string.IsNullOrEmpty(cacheKey) || string.IsNullOrEmpty(cacheValue))
                return;

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = timeToLive
            };
            await _distributedCache.SetStringAsync(cacheKey, cacheValue, options);
        }

        public void CacheResponse<T>(string cacheKey, T cacheValue, TimeSpan timeToLive)
        {
            if (cacheValue == null) return;
            var serializeCacheValue = JsonSerializer.Serialize(cacheValue);
            SetCache(cacheKey, serializeCacheValue, timeToLive);
        }

        public async Task CacheResponseAsync<T>(string cacheKey, T cacheValue, TimeSpan timeToLive)
        {
            if (cacheValue == null) return;

            var serializeCacheValue = JsonSerializer.Serialize(cacheValue);
            await SetCacheAsync(cacheKey, serializeCacheValue, timeToLive);
        }

        public string GetCachedResponse(string cacheKey)
        {
            return _distributedCache.GetString(cacheKey);
        }

        public async Task<string> GetCachedResponseAsync(string cacheKey)
        {
            return await _distributedCache.GetStringAsync(cacheKey);
        }

        public void CacheTupleResponse<T>(string cacheKey, HttpStatusCode statusCode, T cacheValue, TimeSpan timeToLive)
        {
            CacheResponse(cacheKey, cacheValue, timeToLive);
        }

        public async Task CacheTupleResponseAsync<T>(string cacheKey, HttpStatusCode statusCode, T cacheValue, TimeSpan timeToLive)
        {
            if (string.IsNullOrEmpty(cacheKey) || cacheValue is string s && string.IsNullOrEmpty(s))
                return;

            await CacheResponseAsync(cacheKey, cacheValue, timeToLive);
        }

        public (HttpStatusCode, T) GetCachedTupleResponse<T>(string cacheKey)
        {
            var cacheResponse = GetCachedResponse(cacheKey);
            if (string.IsNullOrEmpty(cacheResponse))
                return (HttpStatusCode.BadRequest, default);

            var response = JsonSerializer.Deserialize<T>(cacheResponse);
            return response != null ? (HttpStatusCode.OK, response) : (HttpStatusCode.InternalServerError, default);
        }

        public async Task<(HttpStatusCode, T)> GetCachedTupleResponseAsync<T>(string cacheKey)
        {
            if (string.IsNullOrEmpty(cacheKey))
                return (HttpStatusCode.BadRequest, default);

            var cacheResponse = await GetCachedResponseAsync(cacheKey);
            if (string.IsNullOrEmpty(cacheResponse))
                return (HttpStatusCode.BadRequest, default);

            var response = JsonSerializer.Deserialize<T>(cacheResponse);
            return response != null ? (HttpStatusCode.OK, response) : (HttpStatusCode.InternalServerError, default);
        }

        public string GenerateCacheKey(string methodName, params object[] parameters)
        {
            var language = GetLanguage();
            var key = $"{methodName}";

            if (parameters != null && parameters.Length > 0)
            {
                var parameterHash = string.Join("_", parameters.Select(p => p?.ToString())).GetHashCode();
                key = $"{key}_{parameterHash}";
            }

            return $"{key}:{language}";
        }

        private string GetLanguage()
        {
            var language = _httpContextAccessor.HttpContext?.Items["Language"]?.ToString();
            return string.IsNullOrWhiteSpace(language) ? "ja" : language.ToLower();
        }

        public void ClearAllCache()
        {
            if (_redisConnection == null)
                throw new NotSupportedException("ClearAllCache requires Redis (IConnectionMultiplexer).");

            foreach (var endpoint in _redisConnection.GetEndPoints())
            {
                var server = _redisConnection.GetServer(endpoint);
                server.FlushDatabase(); // Clears all keys
            }
        }

        public async Task ClearAllCacheAsync()
        {
            if (_redisConnection == null)
                throw new NotSupportedException("Redis connection is null.");

            // var servera = _redisConnection.GetServer(_redisConnection.GetEndPoints().First());
            // foreach (var key in servera.Keys())
            // {
            //     Console.WriteLine(key);
            // }

            var db = _redisConnection.GetDatabase();
            var endpoints = _redisConnection.GetEndPoints();

            foreach (var endpoint in endpoints)
            {
                var server = _redisConnection.GetServer(endpoint);

                // Skip replicas – only clear on primaries
                if (server.IsReplica) continue;

                // NOTE: Make sure this prefix matches what you saw in DumpRedisKeys()
                var prefix = _cacheSettings.instanceName;               // e.g. "hoslog.customer.api-DEV"
                var pattern = $"{prefix}*";                             // e.g. "hoslog.customer.api-DEV*"

                var keys = server.Keys(pattern: pattern).ToArray();

                if (!keys.Any())
                    continue;

                var deleteTasks = keys
                    .Select(key => db.KeyDeleteAsync(key))
                    .ToArray();

                await Task.WhenAll(deleteTasks).ConfigureAwait(false);
            }
        }
    }
}
