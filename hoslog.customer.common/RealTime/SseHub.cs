using System.Collections.Concurrent;
using System.Threading.Channels;
using Hoslog.Customer.Common.RealTime.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace hoslog.customer.common.RealTime;

public sealed class SseHub
{
    public sealed record VisitNowActiveSubscription(long locationId, string timeZoneOffsetValue);

    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, Channel<SseCommandEnvelope>>> _topics = new();

    public (Guid connectionId, ChannelReader<SseCommandEnvelope> reader) Subscribe(string topic)
    {
        var connectionId = Guid.NewGuid();

        var channel = Channel.CreateUnbounded<SseCommandEnvelope>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });

        var topicClients = _topics.GetOrAdd(
            topic,
            _ => new ConcurrentDictionary<Guid, Channel<SseCommandEnvelope>>());

        topicClients.TryAdd(connectionId, channel);

        return (connectionId, channel.Reader);
    }

    public void Unsubscribe(string topic, Guid connectionId)
    {
        if (!_topics.TryGetValue(topic, out var topicClients))
            return;

        if (topicClients.TryRemove(connectionId, out var channel))
            channel.Writer.TryComplete();

        if (topicClients.IsEmpty)
            _topics.TryRemove(topic, out _);
    }

    public async Task PublishAsync(
        string topic,
        SseCommandEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        if (!_topics.TryGetValue(topic, out var topicClients))
            return;

        foreach (var client in topicClients)
            await client.Value.Writer.WriteAsync(envelope, cancellationToken);
    }

    public IReadOnlyList<string> GetActiveTopicsByPrefix(string prefix)
    {
        return _topics
            .Where(x => x.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Where(x => !x.Value.IsEmpty)
            .Select(x => x.Key)
            .ToList();
    }

    public IReadOnlyList<VisitNowActiveSubscription> GetActiveVisitNowSubscriptions()
    {
        return _topics
            .Where(x =>
                x.Key.StartsWith(
                    SseTopicNames.VisitNowPrefix,
                    StringComparison.OrdinalIgnoreCase))
            .Where(x => !x.Value.IsEmpty)
            .Select(x =>
            {
                var success =
                    SseTopicNames.TryGetVisitNowSubscription(
                        x.Key,
                        out var locationId,
                        out var timeZoneOffsetValue);

                return new
                {
                    Success = success,
                    locationId = locationId,
                    timeZoneOffsetValue =
                        timeZoneOffsetValue
                };
            })
            .Where(x => x.Success)
            .Select(x => new VisitNowActiveSubscription(
                x.locationId,
                x.timeZoneOffsetValue))
            .Distinct()
            .ToList();
    }
    public IReadOnlyList<string> GetActiveNotificationUserIds()
    {
        return GetActiveTopicsByPrefix(SseTopicNames.NotificationUserPrefix)
            .Select(topic =>
            {
                var success = SseTopicNames.TryGetNotificationUserId(topic, out var encryptedUserId);

                return new
                {
                    success,
                    encryptedUserId
                };
            })
            .Where(x => x.success)
            .Select(x => x.encryptedUserId)
            .Distinct()
            .ToList();
    }

    public IReadOnlyList<long> GetActiveRecommendationGroupLocationIds()
    {
        return GetActiveTopicsByPrefix(SseTopicNames.RecommendationGroupPrefix)
            .Select(topic =>
            {
                var success = SseTopicNames.TryGetRecommendationGroupLocationId(
                    topic,
                    out var locationId);

                return new 
                {
                    success,
                    locationId
                };

            })
            .Where(x => x.success)
            .Select(x => x.locationId)
            .Distinct()
            .ToList();
    }
}