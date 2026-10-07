namespace hoslog.customer.common.Models.ReservationModel;

public class ReservationTimeslotsResponseModel
{
    public string? slotTime { get; set; }
    public long slotDateTimeMS { get; set; }
    public bool isActive { get; set; }
    public string? reason { get; set; }
    public int remainingAvailableReservation { get; set; }
}