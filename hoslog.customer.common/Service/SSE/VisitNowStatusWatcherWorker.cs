using System.Collections.Concurrent;
using Dapper;
using hoslog.customer.common.RealTime;
using hoslog.customer.common.RealTime.Models;
using hoslog.customer.common.RealTime.Models.VisitNow;
using hoslog.customer.common.Utilities;
using hoslog.customer.shared.SSE;
using Hoslog.Customer.Common.RealTime.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace hoslog.customer.common.Service.SSE;

public sealed class VisitNowStatusWatcherWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SseHub _sseHub;
    private readonly ILogger<VisitNowStatusWatcherWorker> _logger;
    private readonly StringCipher _stringCipher;

    private readonly ConcurrentDictionary<string, long> _lastShuffleSlotBySubscription = new();

    public VisitNowStatusWatcherWorker(
        IServiceScopeFactory scopeFactory,
        SseHub sseHub,
        ILogger<VisitNowStatusWatcherWorker> logger,
        StringCipher stringCipher
    )
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
                var activeSubscriptions = _sseHub.GetActiveVisitNowSubscriptions();

                if (activeSubscriptions.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    continue;
                }

                await using var scope = _scopeFactory.CreateAsyncScope();

                SqlHandler sqlHandler =
                    scope.ServiceProvider.GetRequiredService<SqlHandler>();

                foreach (var subscription in activeSubscriptions)
                {
                    if (stoppingToken.IsCancellationRequested)
                        break;

                    try
                    {
                        await ProcessLocationAsync(
                            sqlHandler,
                            subscription.locationId,
                            subscription.timeZoneOffsetValue,
                            stoppingToken);
                    }
                    catch (OperationCanceledException)
                        when (stoppingToken.IsCancellationRequested)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Failed to process Visit Now location {locationId} and timeZoneOffsetValue {timeZoneOffsetValue}.",
                            subscription.locationId, subscription.timeZoneOffsetValue);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Application stopping.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Visit-nnow watcher worker failed.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private bool TryDecryptLocationId(
    string encryptedLocationId,
    out long locationId)
    {
        locationId = 0;

        if (string.IsNullOrWhiteSpace(encryptedLocationId))
            return false;

        try
        {
            var decryptedValue = _stringCipher.Decrypt(encryptedLocationId);

            return long.TryParse(decryptedValue, out locationId) &&
                   locationId > 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to decrypt Visit Now location ID.");

            return false;
        }
    }

    private async Task ProcessLocationAsync(
        SqlHandler sqlHandler,
        long locationId,
        string timeZoneOffsetValue,
        CancellationToken cancellationToken
    )
    {
        await PublishIfShuffleSlotChangedAsync(
            locationId,
            timeZoneOffsetValue,
            cancellationToken);

        var changedRows =
            await RecalculateStatusAsync(
                sqlHandler,
                locationId,
                timeZoneOffsetValue);

        if (changedRows.Count == 0)
            return;

        var changedClubIds = changedRows
            .Select(x => _stringCipher.Encrypt(x.clubId.ToString()))
            .Distinct()
            .ToList();

        var businessDate = changedRows
            .Select(x => x.businessDate)
            .FirstOrDefault();

        string encryptedLocationId = _stringCipher.Encrypt(locationId.ToString());

        var payload = new VisitNowRefreshPayload
        {
            locationId = encryptedLocationId,
            businessDate = businessDate,
            refreshReason = "StatusChanged",
            changedClubIds = changedClubIds
        };

        var envelope = new SseCommandEnvelope
        {
            domain = SseDomains.visitNow,
            command = SseCommands.refetch,
            data = payload
        };

        var topic = SseTopicNames.VisitNowLocation(locationId, timeZoneOffsetValue);

        await _sseHub.PublishAsync(topic, envelope, cancellationToken);
    }

    public async Task<List<VisitNowStatusChangedRow>> RecalculateStatusAsync(
        SqlHandler sqlHandler,
        long locationId,
        string timeZoneOffsetValue
    )
    {
        string proc = "dbo.apiproc_cp_visit_now_status_recalculate";

        var parameters = new DynamicParameters(new
        {
            locationId,
            timeZoneOffsetValue
        });

        return await sqlHandler.ExecuteListAsync<VisitNowStatusChangedRow>(proc, parameters);
    }

    private async Task PublishIfShuffleSlotChangedAsync(
        long locationId,
        string timeZoneOffsetValue,
        CancellationToken cancellationToken
    )
    {
        var subscriptionKey =
            $"{locationId}|{timeZoneOffsetValue}";

        var currentSlot = GetCUrrentShuffleSlot();

        if (!_lastShuffleSlotBySubscription.TryGetValue(
                   subscriptionKey,
                   out var previousSlot))
        {
            _lastShuffleSlotBySubscription.TryAdd(
                subscriptionKey,
                currentSlot);

            return;
        }

        if (previousSlot == currentSlot)
            return;

        _lastShuffleSlotBySubscription[
               subscriptionKey] = currentSlot;

        string encryptedLocationId = _stringCipher.Encrypt(locationId.ToString());

        var payload = new VisitNowRefreshPayload
        {
            locationId = encryptedLocationId,
            refreshReason = "ShuffleSlotChanged"
        };

        var envelope = new SseCommandEnvelope
        {
            domain = SseDomains.visitNow,
            command = SseCommands.refetch,
            data = payload
        };

        var topic = SseTopicNames.VisitNowLocation(
               locationId,
               timeZoneOffsetValue);

        await _sseHub.PublishAsync(topic, envelope, cancellationToken);
    }

    private static long GetCUrrentShuffleSlot()
    {
        var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var minutes = (long)(DateTime.UtcNow - baseDate).TotalMinutes;

        return minutes / 10;
    }
}