using hoslog.customer.common.RealTime.Models;

namespace Hoslog.Customer.Common.RealTime.Models;

public sealed class SseCommandEnvelope
{
    public string id { get; set; } = Guid.NewGuid().ToString("N");
    public int version { get; set; } = 1;
    public string eventName { get; set; } = SseEvents.command;
    public string domain { get; set; } = string.Empty;
    public string command { get; set; } = string.Empty;
    public object? data { get; set; }
    public DateTime generatedUTCDate { get; set; } = DateTime.UtcNow;
}