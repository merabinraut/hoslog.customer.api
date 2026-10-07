namespace hoslog.customer.common.Models.ReservationModel;

public class ReservationScheduleResponseModel
{
    public long date { get; set; }
    public string? englishDay { get; set; }
    public string? japaneseDay { get; set; }
    public string? scheduleLabel { get; set; }
    public string? schedule { get; set; }
    public string? eventType { get; set; }
}
