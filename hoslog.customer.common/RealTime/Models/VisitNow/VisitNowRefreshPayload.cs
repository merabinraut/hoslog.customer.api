namespace hoslog.customer.common.RealTime.Models.VisitNow;

public sealed class VisitNowRefreshPayload
{
    public string? locationId { get; set; }
    public DateTime? businessDate { get; set; }
    public string refreshReason { get; set; } = "StatusChanged";
    public List<string> changedClubIds { get; set; } = new();
}