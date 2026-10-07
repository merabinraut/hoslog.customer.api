namespace hoslog.customer.shared.ReservationModel;

public class BillingDetailsCommonModel : CommonDBResponse
{
    public string? customerUserName { get; set; }
    public string? customerCostAmount { get; set; }
    public string? reservationRemark { get; set; }
}

public class ConfirmationReservationCommon : CommonDBRequest
{
    public string? userId { get; set; }
    public string? clubId { get; set; }
    public string? noOfPeople { get; set; }
    public long selectedDateTime { get; set; }
}

public class ReservationCheckoutRequestCommon : CommonDBRequest
{
    public string? hostIdList { get; set; }
    public string? actionUser { get; set; }
    public string? planId { get; set; }
    public string? paymentType { get; set; }
    public string? userId { get; set; }
    public string? clubId { get; set; }
    public string? noOfPeople { get; set; }
    public long selectedDateTime { get; set; }
}

public class ReservationCheckoutResponseCommon : CommonDBResponse
{
    public string? planAmount { get; set; }
    public string? planTime { get; set; }
    public string? otpCode { get; set; }
    public string? reservationId { get; set; }
}

public class LineGroupAfterReservationCommon : CommonDBRequest
{
    public string? flag { get; set; }
    public string? clubId { get; set; }
    public string? noOfPeople { get; set; }
    public string? planId { get; set; }
    public string? userName { get; set; }
    public string? reservationId { get; set; }
    public long selectedDateTime { get; set; }
    public string? planAmount { get; set; }
    public string? planTime { get; set; }
    public string? otpCode { get; set; }
}