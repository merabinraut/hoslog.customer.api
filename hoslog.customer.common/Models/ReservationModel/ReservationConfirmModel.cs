using FluentValidation;
using hoslog.customer.common.Models.Middleware.AmazonCognitoModel.UserManagement;
using hoslog.customer.shared;

namespace hoslog.customer.common.Models.ReservationModel;

public class ReservationConfirmRequestModel : CommonDTO
{
    public string? clubId { get; set; }
    public int noOfPeople { get; set; }
    public string? hostIds { get; set; }
    public string? planId { get; set; }
    public string? paymentType { get; set; }
    public long visitDateTimeMS { get; set; }
    public string? customerPreferencesJson { get; set; }
    public string? reservationHoldId { get; set; }
}

public class ReservationConfirmRequestValidatorModel : CommonDTOValidator<ReservationConfirmRequestModel>
{
    public ReservationConfirmRequestValidatorModel()
    {
        RuleFor(x => x.hostIds).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CUERES114_HostIdRequired);
        RuleFor(x => x.planId).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CUERES132_PlanIdRequired);
        RuleFor(x => x.clubId).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CUE106_ClubIdRequired);
        RuleFor(x => x.visitDateTimeMS).Must(value => value > 0).WithErrorCode("Rev103").WithMessage("DateTime is required");
        RuleFor(x => x.noOfPeople).Must(value => value > 0).WithErrorCode(ResponseCodes.CUERES129_NumberOfPeopleRequired);
        RuleFor(x => x.paymentType).Must(value => !string.IsNullOrEmpty(value))
            .WithErrorCode(ResponseCodes.CUERES133_PaymentTypeRequired);
    }
}

public class ReservationConfirmResponseModel
{
    public string? lineGroupId { get; set; }
    public long planAmount { get; set; }
    public int planTime { get; set; }
    public string? otpCode { get; set; }
    public string? reservationId { get; set; }
}

public class ReservationDetailsModel
{
    public string? clubId { get; set; }
    public int noOfPeople { get; set; }
    public string? hostIds { get; set; }
    public string? planId { get; set; }
    public string? paymentType { get; set; }
    public long visitDateTimeMS { get; set; }
    public string? customerPreferencesJson { get; set; }
}