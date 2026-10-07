namespace hoslog.customer.shared.ReservationModel;

public class ReservationConfirmRequestCommon : CommonDBRequest
{
    public long clubId { get; set; }
    public string? clubCode { get; set; }
    public int noOfPeople { get; set; }
    public string? hostIds { get; set; }
    public long planId { get; set; }
    public string? paymentType { get; set; }
    public long visitDateTimeMS { get; set; }
    public string? customerPreferencesJson { get; set; }
    public long reservationHoldId { get; set; }
}

public class ReservationConfirmResponseCommon : CommonDBResponse
{
    public string? lineGroupId { get; set; }
    public long planAmount { get; set; }
    public int planTime { get; set; }
    public string? otpCode { get; set; }
    public string? reservationId { get; set; }
    public bool holdExists { get; set; }
}