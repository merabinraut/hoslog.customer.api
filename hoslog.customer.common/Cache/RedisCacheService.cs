using StackExchange.Redis;

namespace hoslog.customer.common.Cache
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly RedisCacheSetting _cacheSettings;

        public RedisCacheService(IConnectionMultiplexer redis, RedisCacheSetting cacheSettings)
        {
            _redis = redis;
            _cacheSettings = cacheSettings;
        }

        public void Subscribe(string channel, Action<string, string> messageHandler)
        {
            var subscriber = _redis.GetSubscriber();
            subscriber.Subscribe(channel, (redisChannel, message) =>
            {
                messageHandler?.Invoke(redisChannel.ToString(), message.ToString());
            });
        }

        public async Task PublishAsync(string channel, string message)
        {
            var subscriber = _redis.GetSubscriber();
            await subscriber.PublishAsync(channel, message).ConfigureAwait(false);
        }

        //Method to clear whole cache DB 
        public async Task ClearCacheAsync()
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            await Task.Run(() => server.FlushAllDatabases()).ConfigureAwait(false);
        }

        //Method to clear cache for a specific instance from DB
        public async Task ClearInstanceCacheAsync()
        {
            var database = _redis.GetDatabase();
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: $"{_cacheSettings.instanceName}*").ToArray();

            if (keys.Any())
            {
                var tasks = keys.Select(key => database.KeyDeleteAsync(key)).ToArray();
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }
    }
}
