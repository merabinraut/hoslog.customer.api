using System;
using System.Collections.Generic;
using System.Text;
using hoslog.customer.common.RealTime;
using hoslog.customer.common.RealTime.Models;
using hoslog.customer.common.RealTime.Models.RecommendationGroup;
using hoslog.customer.common.Utilities;
using hoslog.customer.shared.SSE;
using Hoslog.Customer.Common.RealTime.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace hoslog.customer.common.Service.SSE
{
    public class RecommendationSseWatcherWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly SseHub _sseHub;
        private readonly StringCipher _stringCipher;
        private readonly ILogger<RecommendationSseWatcherWorker> _logger;
        
        public RecommendationSseWatcherWorker(IServiceScopeFactory scopeFactory, SseHub sseHub, StringCipher stringCipher, ILogger<RecommendationSseWatcherWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _sseHub = sseHub;
            _stringCipher = stringCipher;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var activeLocationIds = _sseHub.GetActiveRecommendationGroupLocationIds();

                    if(activeLocationIds.Count == 0)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                        continue;
                    }
                    await using var scope = _scopeFactory.CreateAsyncScope();

                    var sqlHandler = scope.ServiceProvider.GetRequiredService<SqlHandler>();

                    var changes = await GetRecommendationGroupChangesAsync(sqlHandler);

                    foreach(var change in changes)
                    {
                        if (!activeLocationIds.Contains(change.locationId))
                            continue;

                        var encryptedLocationId = _stringCipher.Encrypt(change.locationId.ToString());

                        var payload = new RecommendationRefreshPayload
                        {
                            locationId = encryptedLocationId,
                            refreshReason = "PositionChanged"
                        };
                        var envelope = new SseCommandEnvelope
                        {
                            domain = SseDomains.recommendationGroup,
                            command = SseCommands.refetch,
                            data = payload
                        };
                        var topic = SseTopicNames.RecommendationGroupLocation(change.locationId);

                        await _sseHub.PublishAsync(topic, envelope, stoppingToken);
                    }
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
                catch(OperationCanceledException)
                {
                    return;
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Recommendation group pagination worker failed,");

                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }

        }
       
        private async Task<List<RecommendationGroupChangeRow>> GetRecommendationGroupChangesAsync(SqlHandler sqlHandler)
        {
            const string proc = "dbo.sproc_get_recommendation_group_changes";

            return await sqlHandler.ExecuteListAsync<RecommendationGroupChangeRow>(proc, new Dapper.DynamicParameters());

           
        }
     
        
    }
}
