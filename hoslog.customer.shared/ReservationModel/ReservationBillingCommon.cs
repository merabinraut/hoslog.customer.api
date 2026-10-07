namespace hoslog.customer.shared.ReservationModel;

public class ReservationBillingRequestCommon : CommonDBRequest
{
    public long clubId { get; set; }
    public string? clubCode { get; set; }
    public long planId { get; set; }
    public int noOfPeople { get; set; }
}

public class ReservationBillingResponseCommon : CommonDBResponse
{
    public string? customerUsername { get; set; }
    public long customerCostAmount { get; set; }
    public bool isFirstReservation { get; set; }
    public string? reservationRemark { get; set; }
}

