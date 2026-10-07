namespace hoslog.customer.shared.ReservationModel;

public class ReservationSendOtpRequestCommon : CommonDBRequest
{
    public string? mobileNumber { get; set; }
    public long reservationHoldId { get; set; }
    public string? otpCode { get; set; }
    public string? otpHash { get; set; }
    public string? traceId { get; set; }
    public string? cognitoAuthSession { get; set; }
    public string? username { get; set; }
}

public class ReservationSendOtpResponseCommon : CommonDBResponse
{
    public bool customerExists { get; set; } = false;
    public string? customerUserName { get; set; }
}

public class ReservationReSendOtpRequestCommon : CommonDBRequest
{
    public string? mobileNumber { get; set; }
    public long reservationHoldId { get; set; }
    public string? otpCode { get; set; }
    public string? otpHash { get; set; }
    public string? cognitoAuthSession { get; set; }
}

public class ReservationReSendOtpResponseCommon : CommonDBResponse
{
    public bool customerExists { get; set; } = false;
    public string? customerUserName { get; set; }
}

public class ReservationVerifyOtpRequestCommon : CommonDBRequest
{
    public string? mobileNumber { get; set; }
    public long reservationHoldId { get; set; }
    public string? cognitoPurpose { get; set; }
    public string? submittedOtpHash { get; set; }
    public bool? isViaMultitenant { get; set; } = false;
}

public class ReservationVerifyOtpResponseCommon : CommonDBResponse
{
    public bool customerExists { get; set; } = false;
}

#region  Reservation update cognito session
public class ReservationUpdateCognitoSessionRequestCommon
{
    public long reservationHoldId { get; set; }
    public string? processId { get; set; }
    public string? cognitoAuthSession { get; set; }
}
#endregion