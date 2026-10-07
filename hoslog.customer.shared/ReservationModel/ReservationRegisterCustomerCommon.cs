namespace hoslog.customer.shared.ReservationModel;

public class ReservationRegisterCustomerRequestCommon : CommonDBRequest
{
    public string? nickName { get; set; }
    public string? emailAddress { get; set; }
    public string? mobileNumber { get; set; }
    public string? dob { get; set; }
    public long reservationHoldId { get; set; }
    public bool isViaMultitenant { get; set; } = false;
}

public class ReservationRegisterCustomerResponseCommon : CommonDBResponse
{
    public string? agentId { get; set; }
    public string? mobileNumber { get; set; }
    public string? emailAddress { get; set; }
    public string? password { get; set; }
    public string? cognitoUsername { get; set; }
    public string? userId { get; set; }
    public string? roleType { get; set; }
}

public class ReservationCustomerDetailsUpdateRequestCommon
{
    public string? agentId { get; set; }
    public string? cognitoUserId { get; set; }
}