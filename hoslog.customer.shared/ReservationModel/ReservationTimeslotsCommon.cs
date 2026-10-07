namespace hoslog.customer.shared.ReservationModel;

public class ReservationTimeslotsRequestCommon : CommonDBRequest
{
    public long clubId { get; set; }
    public string? clubCode { get; set; }
    public long date { get; set; }
    public long planId { get; set; }
}

public class ReservationTimeslotsResponseCommon : CommonDBResponse
{
    public string? slotTime { get; set; }
    public long slotDateTimeMS { get; set; }
    public bool isActive { get; set; }
    public string? reason { get; set; }
    public int remainingAvailableReservation { get; set; }
}