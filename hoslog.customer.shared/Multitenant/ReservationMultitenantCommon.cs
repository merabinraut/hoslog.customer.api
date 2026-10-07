namespace hoslog.customer.shared.Multitenant
{
    public class ClubReservationTimeRequestMultitenantCommon : CommonDBRequest
    {
        public string? subDomainName { get; set; }
        public string? clubId { get; set; }
        public long? date { get; set; }
    }

    public class ClubReservationTimeResponseMultitenantCommon : CommonDBResponse
    {
        public long date { get; set; }
        public string clubOpeningTime { get; set; } = null!;
        public string clubClosingTime { get; set; } = null!;
        public string lastEntryTime { get; set; } = null!;
        public string time { get; set; } = null!;
        public string status { get; set; } = null!;
    }

    public class ConfirmationReservationMultitenantCommon : CommonDBRequest
    {
        public string? clubId { get; set; }
        public string? noOfPeople { get; set; }
        public long selectedDateTime { get; set; }
    }

    public class PlanMultitenantCommon : CommonDBResponse
    {
        public string? planId { get; set; }
        public string? planName { get; set; }
        public string? planTime { get; set; }
        public string? planPrice { get; set; }
        public string? strikePrice { get; set; }
        public bool? isStrikeOut { get; set; }
        public string? planNomination { get; set; }
        public string? planRemarks { get; set; }
        public string? planLiquor { get; set; }
        public string? planLiquorJapanese { get; set; }
        public string? planType { get; set; }
    }

    public class HostMultitenantCommon
    {
        public string? clubId { get; set; }
        public string? hostId { get; set; }
        public string? hostNameEnglish { get; set; }
        public string? hostNameJapanese { get; set; }
        public string? hostImage { get; set; }
        public string? hostPosition { get; set; }
        public ImageResponse? hostImageResponse { get; set; } = new ImageResponse();
    }



    public class ClubSpecificPlanResponseCommon:CommonDBResponse
    {
        public int? planId { get; set; }
        public int? clubId { get; set; }
        public string? planName { get; set; }
        public string? planTime { get; set; }
        public string? planPrice { get; set; }
        public string? noOfPeople { get; set; }
        public string? noOfNomination { get; set; }
    }

    #region Reservation Confirmation
    public class ConfirmReservationMultitenantRequestCommon : CommonDBRequest
    {
        public string? clubId { get; set; }
        public string? planId { get; set; }
        public int noOfPeople { get; set; }
        public string? hostIdList { get; set; }
        public string? paymentType { get; set; }
        public long selectedDateTime { get; set; }
        public string? referCode { get; set; }
        public string? referType { get; set; }
    }

    public class ConfirmReservationMultitenantResponseCommon : CommonDBResponse
    {
        public string? reservationHoldId { get; set; }
        public string? reservationTraceId { get; set; }
        public string? clubName { get; set; }
        public string? clubNameEng { get; set; }
        public string? clubLogo { get; set; }
        public string? clubCode { get; set; }
    }
    #endregion

    #region Get reservation details and send otp
    public class GetHoldReservationAndSendOtpRequestCommon : CommonDBRequest
    {
        public string? mobileNumber { get; set; }
        public string? reservationHoldId { get; set; }
        public string? reservationTraceId { get; set; }
    }

    public class GetHoldReservationAndSendOtpResponseCommon : CommonDBResponse
    {
        public string? reservationHoldId { get; set; }
        public string? reservationTraceId { get; set; }
        public string? customerId { get; set; }
        public bool? customerExists { get; set; }
    }
    #endregion

    #region Reservation hold details
    public class ReservationHoldDetailMultitenantResponseCommon : CommonDBResponse
    {
        public string? reservationHoldId { get; set; }
        public string? reservationTraceId { get; set; }
        public string? reservationId { get; set; }
        public string? clubId { get; set; }
        public string? customerId { get; set; }
        public string? reservedDate { get; set; }
        public string? reservedDateUTC { get; set; }
        public string? visitDate { get; set; }
        public string? visitTime { get; set; }
        public string? visitUTCDateTime { get; set; }
        public int noOfPeople { get; set; }
        public string? paymentType { get; set; }
        public string? planId { get; set; }
        public string? hostId { get; set; }
        public string? otpCode { get; set; }
        public bool isOTPCodeVerified { get; set; }
        public int otpRetryCount { get; set; }
        public string? createdBy { get; set; }
        public DateTime createdDate { get; set; }
        public DateTime createdUTCDate { get; set; }
        public string? updatedBy { get; set; }
        public DateTime? updatedDate { get; set; }
        public DateTime? updatedUTCDate { get; set; }
        public string? clubStatus { get; set; }
    }
    #endregion

    #region Confirm reservation model
    public class ConfirmReservationHoldMultitenantRequestCommon : CommonDBRequest
    {
        public string? reservationHoldId { get; set; }
        public string? reservationTraceId { get; set; }
        public string? customerId { get; set; }
    }

    public class ConfirmReservationHoldMultitenantResponseCommon : CommonDBResponse
    {
        public string? lineGroupId { get; set; }
        public string? planAmount { get; set; }
        public string? planTime { get; set; }
        public string? clubId { get; set; }
        public string? planId { get; set; }
        public string? customerUserName { get; set; }
        public string? noOfPeople { get; set; }
        public string? otpCode { get; set; }
        public long visitDateTime { get; set; }
        public string? reservationId { get; set; }
    }
    #endregion

    #region Registration
    public class CustomerRegistrationMultitenantRequestCommon : CommonDBRequest
    {
        public string? nickName { get; set; }
        public string? emailAddress { get; set; }
        public string? dob { get; set; }
        public string? mobileNumber { get; set; }
        public string? reservationHoldId { get; set; }
        public string? reservationTraceId { get; set; }
    }

    public class CustomerRegistrationMultitenantResponseCommon : CommonDBResponse
    {
        public string? customerId { get; set; }
        public string? mobileNumber { get; set; }
        public string? nickName { get; set; }
        public string? password { get; set; }
        public string? cognitoUsername { get; set; }
    }

    public class CustomerDetailUpdateMultitenantRequestCommon
    {
        public string? agentId { get; set; }
        public string? cognitoUserId { get; set; }
    }
    #endregion

    #region  OTP Verification
    public class OTPVerificationMultitenantRequestCommon : CommonDBRequest
    {
        public string? reservationHoldId { get; set; }
        public string? reservationTraceId { get; set; }
        public string? mobileNumber { get; set; }
        public string? otpCode { get; set; }
    }

    public class OTPVerificationMultitenantResponseCommon : CommonDBResponse
    {
        public string? reservationHoldId { get; set; }
        public string? reservationTraceId { get; set; }
        public string? mobileNumber { get; set; }
    }
    #endregion
}