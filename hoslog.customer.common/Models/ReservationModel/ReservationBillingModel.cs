namespace hoslog.customer.common.Models.ReservationModel;

public class ReservationBillingResponseModel
{
    public string? customerUsername { get; set; }
    public long customerCostAmount { get; set; }
    public bool isFirstReservation { get; set; }
    public string? reservationRemark { get; set; }
}

