namespace hoslog.customer.shared.ReservationModel;

public class ReservationScheduleRequestCommon : CommonDBRequest
{
    public long clubId { get; set; }
    public string? clubCode { get; set; }
    public int duration { get; set; }
}

public class ReservationScheduleResponseCommon : CommonDBResponse
{
    public long date { get; set; }
    public string? englishDay { get; set; }
    public string? japaneseDay { get; set; }
    public string? scheduleLabel { get; set; }
    public string? schedule { get; set; }
    public string? eventType { get; set; }
}
