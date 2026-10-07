namespace hoslog.customer.shared.SSE;

public sealed class VisitNowStatusChangedRow
{
    public int locationId { get; set; }
    public DateTime businessDate { get; set; }
    public long clubId { get; set; }
    public bool? oldIsReservable { get; set; }
    public bool newIsReservable { get; set; }
    public string? oldClubSchedule { get; set; }
    public string newClubSchedule { get; set; } = string.Empty;
}
