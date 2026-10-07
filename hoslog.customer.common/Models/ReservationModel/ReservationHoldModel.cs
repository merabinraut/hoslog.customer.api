using FluentValidation;
using hoslog.customer.shared;

namespace hoslog.customer.common.Models.ReservationModel;

public class ReservationHoldConfirmRequestModel : CommonDTO
{
    public string? clubId { get; set; }
    public int noOfPeople { get; set; }
    public string? hostIds { get; set; }
    public string? planId { get; set; }
    public long visitDateTimeMS { get; set; }
    public string? customerPreferencesJson { get; set; }
    public string? mobileNumber { get; set; }
    public string? referCode { get; set; }
    public string? referType { get; set; }
}

public class ReservationHoldConfirmResponseModel
{
    public bool canReserve { get; set; } = true;
    public long previousVisitDate { get; set; }
    public long nextEligibleDate { get; set; }
    public string? reservationHoldId { get; set; }
    public bool isViaMultitenant { get; set; } = false;
    public bool hasMultitenantAffiliate { get; set; } = false;
    public bool doesCustomerExists { get; set; } = false;
    public string? processId { get; set; }
    public long expiryTime { get; set; }
}

public class ReservationHoldReSendOtpRequestModel : CommonDTO
{
    public string? mobileNumber { get; set; }
    public string? reservationHoldId { get; set; }
}

public class ReservationHoldReSendOtpRequestModelValidatorModel : CommonDTOValidator<ReservationHoldReSendOtpRequestModel>
{
    public ReservationHoldReSendOtpRequestModelValidatorModel()
    {
        RuleFor(x => x.mobileNumber).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CE469_MobileNumberRequired);
        RuleFor(x => x.reservationHoldId).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CUERES114_HostIdRequired);
    }
}

public class ReservationHoldReSendOtpResponseModel
{
    public string? reservationHoldId { get; set; }
    public bool isViaMultitenant { get; set; } = false;
    public bool hasMultitenantAffiliate { get; set; } = false;
    public bool doesCustomerExists { get; set; } = false;
    public string? processId { get; set; }
    public long expiryTime { get; set; }
}

public class ReservationHoldConfirmRequestValidatorModel : CommonDTOValidator<ReservationHoldConfirmRequestModel>
{
    public ReservationHoldConfirmRequestValidatorModel()
    {
        RuleFor(x => x.mobileNumber).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CE469_MobileNumberRequired);
        RuleFor(x => x.hostIds).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CUERES114_HostIdRequired);
        RuleFor(x => x.planId).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CUERES132_PlanIdRequired);
        RuleFor(x => x.clubId).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CUE106_ClubIdRequired);
        RuleFor(x => x.visitDateTimeMS).Must(value => value > 0).WithErrorCode("Rev103").WithMessage("DateTime is required");
        RuleFor(x => x.noOfPeople).Must(value => value > 0).WithErrorCode(ResponseCodes.CUERES129_NumberOfPeopleRequired);
    }
}