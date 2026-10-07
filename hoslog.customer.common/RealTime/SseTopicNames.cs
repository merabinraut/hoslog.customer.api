using hoslog.customer.common.Utilities;

namespace hoslog.customer.common.RealTime;

public static class SseTopicNames
{
    public const string VisitNowPrefix = "visit-now:location:";
    private const string TimeZoneMarker = ":timezone:";
    public const string NotificationUserPrefix = "notification:user:";
    public const string RecommendationGroupPrefix = "recommendation-group:location:";

    public static string VisitNowLocation(long locationId, string timeZoneOffsetValue)
    {
        return $"{VisitNowPrefix}{locationId}" +
               $"{TimeZoneMarker}{timeZoneOffsetValue}";
    }

    public static string NotificationUser(string userId)
    {
        return $"{NotificationUserPrefix}{userId}";
    }

    public static string RecommendationGroupLocation(long locationId)
    {
        return $"{RecommendationGroupPrefix}{locationId}";
    }

    public static bool TryGetVisitNowSubscription(
           string topic,
           out long locationId,
           out string timeZoneOffsetValue)
    {
        locationId = 0;
        timeZoneOffsetValue = string.Empty;

        if (string.IsNullOrWhiteSpace(topic))
        {
            return false;
        }

        if (!topic.StartsWith(
                VisitNowPrefix,
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var topicValue =
            topic[VisitNowPrefix.Length..];

        var markerIndex = topicValue.IndexOf(
            TimeZoneMarker,
            StringComparison.OrdinalIgnoreCase);

        if (markerIndex <= 0)
        {
            return false;
        }

        var locationPart =
            topicValue[..markerIndex];

        var timezonePart =
            topicValue[
                (markerIndex + TimeZoneMarker.Length)..];

        if (!long.TryParse(
                locationPart,
                out locationId)
            || locationId <= 0)
        {
            return false;
        }

        if (!TimeZoneOffsetHelper.TryNormalize(
                timezonePart,
                out timeZoneOffsetValue))
        {
            return false;
        }

        return true;
    }
    public static bool TryGetNotificationUserId(string topic, out string userId)
    {
        userId = string.Empty;

        if (string.IsNullOrWhiteSpace(topic) || !topic.StartsWith(NotificationUserPrefix, StringComparison.OrdinalIgnoreCase))
            return false;

        userId = topic[NotificationUserPrefix.Length..];
        return !string.IsNullOrWhiteSpace(userId);
    }

    public static bool TryGetRecommendationGroupLocationId(string topic, out long locationId)
    {
        locationId = 0;

        if (string.IsNullOrWhiteSpace(topic) || !topic.StartsWith(RecommendationGroupPrefix, StringComparison.OrdinalIgnoreCase))
            return false;

        var locationPart = topic[RecommendationGroupPrefix.Length..];

        return long.TryParse(locationPart, out locationId) && locationId > 0;
    }
}