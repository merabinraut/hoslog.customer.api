namespace hoslog.customer.shared.ReservationModel;

public class ReservationValidateRequestCommon : CommonDBRequest
{
    public long clubId { get; set; }
    public string? clubCode { get; set; }
    public long planId { get; set; }
    public long selectedDateTime { get; set; }
}

