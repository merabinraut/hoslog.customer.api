using Amazon.S3.Model;
using Dapper;
using hoslog.customer.common.Models;
using hoslog.customer.common.RealTime;
using hoslog.customer.common.RealTime.Models;
using hoslog.customer.common.RealTime.Models.Notification;
using hoslog.customer.common.Utilities;
using hoslog.customer.shared;
using hoslog.customer.shared.SSE;
using Hoslog.Customer.Common.RealTime.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace hoslog.customer.common.Service.SSE
{
    public sealed class NotificationSseWatcherWorker : BackgroundService
    {
        private const int CustomerRoleType = 3;
        private const int FetchSize = 50;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly SseHub _sseHub;
        private readonly ILogger<NotificationSseWatcherWorker> _logger;
        private readonly StringCipher _stringCipher;

        public NotificationSseWatcherWorker(IServiceScopeFactory scopeFactory, SseHub sseHub, ILogger<NotificationSseWatcherWorker> logger, StringCipher stringCipher)
        {
            _scopeFactory = scopeFactory;
            _sseHub = sseHub;
            _logger = logger;
            _stringCipher = stringCipher;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using var scope = _scopeFactory.CreateAsyncScope();

                    SqlHandler sqlHandler = scope.ServiceProvider.GetRequiredService<SqlHandler>();

                    var rows = await GetPendingNotificationOutboxAsync(sqlHandler, CustomerRoleType, FetchSize);

                    if(rows.Count == 0)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                        continue;
                    }
                    foreach(var row in rows)
                    {
                        if (stoppingToken.IsCancellationRequested)
                            break;
                        try
                        {
                            await ProcessNotificationAsync(sqlHandler , row, stoppingToken);
                        }
                        catch(OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                        {
                            return;
                        }
                        catch(Exception ex)
                        {
                            _logger.LogError(ex, "Failed to process notification outbox{OutboxId}.", row.outboxId);
                            await MarkNotificationOutboxFailedAsync(sqlHandler, row.outboxId, ex.Message);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    //application stopping
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Notification SSE watcher worker failed.");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        private async Task ProcessNotificationAsync(SqlHandler sqlHandler , NotificationSseOutboxRow row, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(row.userId))
            {
                await MarkNotificationOutboxFailedAsync(sqlHandler, row.outboxId, "UserId not found for notification recipient.");
                return;
            }
            string encryptedUserId = _stringCipher.Encrypt(row.userId);

            var payload = new NotificationRefreshPayload
            {
                userId = encryptedUserId,
                notification = new NotificationDetailCommon
                {
                    notificationId = _stringCipher.Encrypt(row.notificationId.ToString()),
                    notificationTo = _stringCipher.Encrypt(row.notificationTo.ToString()),
                    notificationType = row.notificationType,
                    notificationSubject = row.notificationSubject,
                    notificationBody = row.notificationBody,
                    notificationStatus = row.notificationStatus,
                    notificationReadStatus = row.notificationReadStatus,
                    allNotificationCount = row.allNotificationCount,
                    dateCategory = row.dateCategory,
                    formattedDate = row.formattedDate,
                    notificationURL = row.notificationURL,
                    unReadNotificationCount = row.unReadNotificationCount,
                    createdDate = row.createdDate,
                    additionalDetail1 = string.IsNullOrEmpty(row.additionalDetail1) ? string.Empty : _stringCipher.Encrypt(row.additionalDetail1),
                    additionalDetail2 = string.IsNullOrEmpty(row.additionalDetail2) ? string.Empty: _stringCipher.Encrypt(row.additionalDetail2),
                    additionalDetail3 = string.IsNullOrEmpty(row.additionalDetail3)? string.Empty: row.additionalDetail3,
                    image = new ImageResponse
                    {
                        alt = row.notificationSubject,
                        path = row.notificationImage,
                        description = null
                    }
                },
                refreshReason = "NotificationCreated"
            };

            var envelope = new SseCommandEnvelope
            {
                domain = SseDomains.notification,
                command = SseCommands.append,
                data = payload,
                generatedUTCDate = DateTime.UtcNow
            };

            var topic = SseTopicNames.NotificationUser(row.userId);

            await _sseHub.PublishAsync(topic, envelope, cancellationToken);
            await MarkNotificationOutboxProcessedAsync(sqlHandler, row.outboxId);
        }

        public async Task<List<NotificationSseOutboxRow>> GetPendingNotificationOutboxAsync(SqlHandler sqlHandler, int roleType, int fetch)
        {
            string proc = "dbo.apiproc_cp_notification_sse_get_pending";

            var parameters = new DynamicParameters(new
            {
                roleType,
                fetch
            });

            return await sqlHandler.ExecuteListAsync<NotificationSseOutboxRow>(proc, parameters);
        }

        public async Task MarkNotificationOutboxProcessedAsync(SqlHandler sqlHandler, long outboxId)
        {
            string proc = "dbo.apiproc_cp_notification_sse_mark_processed";
            var parameters = new DynamicParameters(new
            {
                outboxId
            });

            await sqlHandler.ExecuteAsync<CommonDBResponse>(proc, parameters);
        }
        public async Task MarkNotificationOutboxFailedAsync(SqlHandler sqlHandler, long outboxId, string errorMessage)
        {
            string proc = "dbo.apiproc_cp_notification_sse_mark_failed";

            var parameters = new DynamicParameters(new
            {
                outboxId,
                errorMessage
            });
            await sqlHandler.ExecuteAsync<CommonDBResponse>(proc, parameters);

        }
        
    }
}
