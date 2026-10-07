using hoslog.customer.common.Cache;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hoslog.customer.common.Service
{
    public class RedisSubscriptionService : BackgroundService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly RedisCacheSetting _cacheSettings;
        private readonly IResponseCacheService _cacheService;
        private readonly string _channel;
        public RedisSubscriptionService(IConnectionMultiplexer redis, RedisCacheSetting cacheSettings, IResponseCacheService cacheService)
        {
            _redis = redis;
            _cacheSettings = cacheSettings;
            _cacheService = cacheService;
            _channel = _cacheSettings?.channel;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var subscriber = _redis.GetSubscriber();

            await subscriber.SubscribeAsync(_channel, async (channel, message) =>
            {
                var data = message.ToString().Split('|');
                if (data.Length == 2)
                {
                    var cacheKey = data[0];
                    var cacheValue = data[1];

                    if (!string.IsNullOrEmpty(cacheKey) && !string.IsNullOrEmpty(cacheValue) && _cacheSettings.isEnabled)
                    {
                        await _cacheService.SetCacheAsync(cacheKey, cacheValue, TimeSpan.FromMinutes(_cacheSettings.maximumIntervalInMinutes));
                        Console.WriteLine($"Received message from {channel}: {message}");
                    }
                }
            }).ConfigureAwait(false);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken).ConfigureAwait(false);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Stopping redis subscription service...");
            await base.StopAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
