namespace hoslog.customer.shared.ReservationModel;

public class ReservationHoldConfirmRequestCommon : ReservationConfirmRequestCommon
{
    public string? mobileNumber { get; set; }
    public bool isViaMultitenant { get; set; } = false;
    public string? referCode { get; set; }
    public string? referType { get; set; }
}

public class ReservationHoldConfirmResponseCommon : CommonDBResponse
{
    public bool canReserve { get; set; } = true;
    public long previousVisitDate { get; set; }
    public long nextEligibleDate { get; set; }
    public long reservationHoldId { get; set; }
    public string? otpCode { get; set; }
    public bool isViaMultitenant { get; set; } = false;
    public bool hasMultitenantAffiliate { get; set; } = false;
    public bool doesCustomerExists { get; set; } = false;
    public string? customerUsername { get; set; }
    public string? customerCognitoUsername { get; set; }
    public string? processId { get; set; }
}

public class ReservationHoldDetailResponseCommon : CommonDBResponse
{
    public long reservationHoldId { get; set; }
    public string? processId { get; set; }
    public long reservationId { get; set; }
    public long clubId { get; set; }
    public long customerId { get; set; }
    public long reservedDateTimeMS { get; set; }
    public long visitDateTimeMS { get; set; }
    public int noOfPeople { get; set; }
    public string? paymentType { get; set; }
    public long planId { get; set; }
    public string? hostId { get; set; }
    public bool isOTPCodeVerified { get; set; } = false;
    public long referId { get; set; }
    public long affiliateId { get; set; }
    public string? referType { get; set; }
    public string? customerPreferencesJson { get; set; }
    public string? customAuthUsername { get; set; }
    public string? customAuthPurpose { get; set; }
    public string? customAuthOtpHash { get; set; }
    public string? cognitoAuthSession { get; set; }
    public string? roleType { get; set; }
    public string? userId { get; set; }
    public bool hasMultitenantAffiliate { get; set; } = false;
    public bool doesCustomerExists { get; set; } = false;
}