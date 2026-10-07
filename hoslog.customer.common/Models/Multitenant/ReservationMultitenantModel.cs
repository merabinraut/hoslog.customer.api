using FluentValidation;
using hoslog.customer.shared;

namespace hoslog.customer.common.Models.Multitenant
{
    public class ClubReservationTimeResponseMultitenantModel
    {
        public long date { get; set; }
        public string clubOpeningTime { get; set; } = null!;
        public string clubClosingTime { get; set; } = null!;
        public string lastEntryTime { get; set; } = null!;
        public List<ClubReservationTimeDetailMultitenantModel> clubReservationTimeDetailModel { get; set; } = new List<ClubReservationTimeDetailMultitenantModel>();
    }

    public class ClubReservationTimeDetailMultitenantModel
    {
        public string time { get; set; } = null!;
        public string status { get; set; } = null!;
    }

    public class ConfirmationReservationMultitenantModel : CommonDTO
    {
        public string? clubId { get; set; }
        public string? noOfPeople { get; set; }
        public long selectedDateTime { get; set; }
    }
    public class ConfirmationReservationMultitenantModelValidator : CommonDTOValidator<ConfirmationReservationMultitenantModel>
    {
        public ConfirmationReservationMultitenantModelValidator()
        {
            RuleFor(x => x.clubId).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode("Rev100").WithMessage("ClubId is required");
            RuleFor(x => x.noOfPeople).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode("Rev102").WithMessage("Number of people is required");
            RuleFor(x => x.selectedDateTime).Must(value => value > 0).WithErrorCode("Rev103").WithMessage("DateTime is required");
        }
    }

    public class CommonClubAndPlanMultitenantModel
    {
        public List<PlanMultitenantModel>? planList { get; set; } = null;
        public IList<ClubBasicDetailMultitenantModel>? clubDetails { get; set; } = null;
        public List<HostMultitenantModel>? hostList { get; set; } = null;
        public List<ClubPlanResponseModel>? clubPlanList { get; set; } = null;
    }

    public class ClubBasicDetailMultitenantModel
    {
        public string? clubId { get; set; }
        public string? clubLogo { get; set; }
        public string? clubNameEnglish { get; set; }
        public string? clubNameJapanese { get; set; }
        public string? clubLocationName { get; set; }
        public ImageResponse? clubLogoResponse { get; set; } = new ImageResponse();
    }

    public class PlanMultitenantModel
    {
        public string? code { get; set; }
        public string? message { get; set; }
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
    public class PlanResponseMultitenantModel
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

    public class HostMultitenantModel
    {
        public string? clubId { get; set; }
        public string? hostId { get; set; }
        public string? hostNameEnglish { get; set; }
        public string? hostNameJapanese { get; set; }
        public string? hostImage { get; set; }
        public string? hostPosition { get; set; }
        public ImageResponse? hostImageResponse { get; set; } = new ImageResponse();
    }


    public class ClubPlanResponseModel
    {
        public string? planId { get; set; }
        public string? clubId { get; set; }
        public string? planName { get; set; }
        public string? planTime { get; set; }
        public string? planPrice { get; set; }
        public string? noOfPeople { get; set; }
        public string? noOfNomination { get; set; }
    }

    #region Reservation
    public class ConfirmReservationMultitenantRequestModel : ConfirmationReservationMultitenantModel
    {
        public string? planId { get; set; }
        public string? hostIdList { get; set; }
        public string? paymentType { get; set; }
        public string? referCode { get; set; }
        public string? referType { get; set; }
    }

    public class ConfirmReservationMultitenantRequestValidatorModel : CommonDTOValidator<ConfirmReservationMultitenantRequestModel>
    {
        public ConfirmReservationMultitenantRequestValidatorModel()
        {
            RuleFor(x => x.hostIdList).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CUERES114_HostIdRequired);
            RuleFor(x => x.planId).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CUERES132_PlanIdRequired);
            RuleFor(x => x.clubId).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CUE106_ClubIdRequired);
            RuleFor(x => x.selectedDateTime).Must(value => value > 0).WithErrorCode(ResponseCodes.CUE124_DateTimeRequired);
            RuleFor(x => x.noOfPeople).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CUERES129_NumberOfPeopleRequired);
            // RuleFor(x => x.referCode).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode("Refer code required.");
            // RuleFor(x => x.referType).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode("Refer type required");
        }
    }

    public class ConfirmReservationMultitenantResponseModel
    {
        public string? reservationHoldId { get; set; }
        public string? reservationTraceId { get; set; }
        public string? clubName { get; set; }
        public string? clubNameEng { get; set; }
        public string? clubCode { get; set; }
        public ImageResponse clubLogo { get; set; } = new ImageResponse();
    }
    #endregion

    #region Get reservation details and send otp
    public class GetHoldReservationAndSendOtpRequestModel : CommonDTO
    {
        public string? mobileNumber { get; set; }
        public string? reservationHoldId { get; set; }
        public string? reservationTraceId { get; set; }
    }

    public class GetHoldReservationAndSendOtpRequestValidatorModel : CommonDTOValidator<GetHoldReservationAndSendOtpRequestModel>
    {
        public GetHoldReservationAndSendOtpRequestValidatorModel()
        {
            RuleFor(x => x.reservationHoldId).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CUERES138_ReservationHoldIdRequired);
            RuleFor(x => x.reservationTraceId).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CUERES139_ReservationTraceIdRequired);
            RuleFor(x => x.mobileNumber).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CE469_MobileNumberRequired);
        }
    }

    public class GetHoldReservationAndSendOtpResponseModel
    {
        public string? reservationHoldId { get; set; }
        public string? reservationTraceId { get; set; }
        public string? customerId { get; set; }
        public string? mobileNumber { get; set; }
        public bool? customerExists { get; set; }
        public int expiryTime { get; set; }
    }
    #endregion

    #region Confirm reservation hold
    public class ConfirmReservationHoldMultitenantRequestModel : CommonDTO
    {
        public string? reservationHoldId { get; set; }
        public string? reservationTraceId { get; set; }
        public string? customerId { get; set; }
        public string? otpCode { get; set; }
    }

    public class ConfirmReservationHoldMultitenantRequestValidatorModel : CommonDTOValidator<ConfirmReservationHoldMultitenantRequestModel>
    {
        public ConfirmReservationHoldMultitenantRequestValidatorModel()
        {
            RuleFor(x => x.reservationHoldId).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CUERES138_ReservationHoldIdRequired);
            RuleFor(x => x.reservationTraceId).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CUERES139_ReservationTraceIdRequired);
            RuleFor(x => x.customerId).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CE465_CustomerIdRequired);
            RuleFor(x => x.otpCode).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CE470_OTPCodeRequired);
        }
    }
    #endregion

    #region Registration
    public class NonExistingCustomerReservationConfirmMultitenantRequestCommon : CommonDTO
    {
        public string? nickName { get; set; }
        public string? emailAddress { get; set; }
        public string? dob { get; set; }
        public string? mobileNumber { get; set; }
        public string? reservationHoldId { get; set; }
        public string? reservationTraceId { get; set; }
    }

    public class NonExistingCustomerReservationConfirmMultitenantRequestValidatorModel : CommonDTOValidator<NonExistingCustomerReservationConfirmMultitenantRequestCommon>
    {
        public NonExistingCustomerReservationConfirmMultitenantRequestValidatorModel()
        {
            RuleFor(x => x.nickName).Must(value => !string.IsNullOrWhiteSpace(value)).WithErrorCode(ResponseCodes.CE468_NicknameRequired);
            RuleFor(x => x.emailAddress).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CodeCUEREG116_Email_Address_is_required)
                .EmailAddress().WithErrorCode(ResponseCodes.CodeCUEREG118_Invalid_Email_Format);
            RuleFor(x => x.dob).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CodeCUEREG117_DOB_is_required);
            RuleFor(x => x.mobileNumber).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CE469_MobileNumberRequired);
        }
    }
    #endregion

    #region  OTP Verification
    public class OTPVerificationMultitenantRequestModel : CommonDTO
    {
        public string? reservationHoldId { get; set; }
        public string? reservationTraceId { get; set; }
        public string? mobileNumber { get; set; }
        public string? otpCode { get; set; }
    }

    public class OTPVerificationMultitenantRequestValidatorModel : CommonDTOValidator<OTPVerificationMultitenantRequestModel>
    {
        public OTPVerificationMultitenantRequestValidatorModel()
        {
            RuleFor(x => x.reservationHoldId).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CUERES138_ReservationHoldIdRequired);
            RuleFor(x => x.reservationTraceId).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CUERES139_ReservationTraceIdRequired);
            RuleFor(x => x.mobileNumber).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CE469_MobileNumberRequired);
            RuleFor(x => x.otpCode).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CE470_OTPCodeRequired);
        }
    }

    public class OTPVerificationMultitenantResponseModel
    {
        public string? reservationHoldId { get; set; }
        public string? reservationTraceId { get; set; }
        public string? mobileNumber { get; set; }
    }
    #endregion
}