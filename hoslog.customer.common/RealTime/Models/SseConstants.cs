namespace hoslog.customer.common.RealTime.Models;

public static class SseEvents
{
    public const string command = "sse-command";
}

public static class SseDomains
{
    public const string visitNow = "visitNow";
    public const string notification = "notification";
    public const string recommendationGroup = "recommendationGroup";
}

public static class SseCommands
{
    public const string refetch = "REFETCH";
    public const string toast = "TOAST";
    public const string append = "APPEND";
    public const string update = "UPDATE";
    public const string remove = "REMOVE";
}