using FluentValidation;
using hoslog.customer.shared;

namespace hoslog.customer.common.Models.ReservationModel;

public class CommonClubAndPlanModel
{
    public IList<PlanModel>? planList { get; set; } = null;
    public IList<ClubBasicDetailModel>? clubDetails { get; set; } = null;
    public IList<HostModel>? hostList { get; set; } = null;
}
public class ConfirmationReservationModel : CommonDTO
{
    public string? userId { get; set; }
    public string? clubId { get; set; }
    public string? noOfPeople { get; set; }
    public long selectedDateTime { get; set; }
}
public class ConfirmationReservationValidator : CommonDTOValidator<ConfirmationReservationModel>
{
    public ConfirmationReservationValidator()
    {
        RuleFor(x => x.clubId).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode("Rev100").WithMessage("ClubId is required");
        RuleFor(x => x.noOfPeople).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode("Rev102").WithMessage("Number of people is required");
        RuleFor(x => x.selectedDateTime).Must(value => value > 0).WithErrorCode("Rev103").WithMessage("DateTime is required");
    }
}
public class CommonReservationModel : ConfirmationReservationModel
{

}
public class ClubBasicDetailModel
{
    public string? clubId { get; set; }
    public string? clubLogo { get; set; }
    public string? clubNameEnglish { get; set; }
    public string? clubNameJapanese { get; set; }
    public string? clubLocationName { get; set; }
    public ImageResponse? clubLogoResponse { get; set; } = new ImageResponse();
}
public class PlanModel : ResponseStatus
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
public class HostModel
{
    public string? clubId { get; set; }
    public string? hostId { get; set; }
    public string? hostNameEnglish { get; set; }
    public string? hostNameJapanese { get; set; }
    public string? hostImage { get; set; }
    public string? hostPosition { get; set; }
    public ImageResponse? hostImageResponse { get; set; } = new ImageResponse();
}
public class BillingDetailsModel
{
    public string? customerUserName { get; set; }
    public string? customerCostAmount { get; set; }
    public string? reservationRemark { get; set; }
}
public class BilligDetailRequestModel : CommonDTO
{
    public string? clubId { get; set; }
    public string? userId { get; set; }
    public string? planId { get; set; }
    public int? noOfPeople { get; set; }
}
public class BillingDetailRequestModelValidator : CommonDTOValidator<BilligDetailRequestModel>
{
    public BillingDetailRequestModelValidator()
    {
        RuleFor(x => x.clubId).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode("Bill100").WithMessage("ClubId is required");
        RuleFor(x => x.planId).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode("Bill100").WithMessage("PlanId is required");
        RuleFor(x => x.noOfPeople).Must(value => !string.IsNullOrEmpty(value.ToString())).WithErrorCode("Bill100").WithMessage("No of people is required");

    }
}
public class PaymentMethodModel
{
    public string? staticValue { get; set; }
    public string? staticLabel { get; set; }
    public string? staticLabelEng { get; set; }
    public string? staticLabelJp { get; set; }
    public long customerPoints { get; set; } = 0;
    public bool allowPayment { get; set; } = true;
}

public class PaymentMethodResponseModel
{
    public string? staticValue { get; set; }
    public string? staticLabel { get; set; }
    public string? staticLabelEng { get; set; }
    public string? staticLabelJp { get; set; }
    public string customerPoints { get; set; } = string.Empty;
    public bool allowPayment { get; set; } = true;
}
public class ReservationCheckoutRequestModel : ConfirmationReservationModel
{
    public string? hostIdList { get; set; }
    public string? actionUser { get; set; }
    public string? actionIP { get; set; }
    public string? planId { get; set; }
    public string? paymentType { get; set; }
}
public class ReservationCheckoutRequestValidatorModel : CommonDTOValidator<ReservationCheckoutRequestModel>
{
    public ReservationCheckoutRequestValidatorModel()
    {
        RuleFor(x => x.hostIdList).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CUERES114_HostIdRequired);
        RuleFor(x => x.planId).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CUERES132_PlanIdRequired);
        // RuleFor(x => x.paymentType).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CUERES133_PaymentTypeRequired);
        RuleFor(x => x.clubId).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CUE106_ClubIdRequired);
        // RuleFor(x => x.selectedDate).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CUERES131_ReservationDateRequired);
        // RuleFor(x => x.selectedTime).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CUERES128_ReservationTimeRequired);
        RuleFor(x => x.selectedDateTime).Must(value => value > 0).WithErrorCode("Rev103").WithMessage("DateTime is required");
        RuleFor(x => x.noOfPeople).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CUERES129_NumberOfPeopleRequired);
    }
}
public class ClubReservationTimeRequestModel
{
    public string clubId { get; set; } = null!;
    public string? date { get; set; }
}

public class ClubReservationTimeResponseModel
{
    public long date { get; set; }
    public string clubOpeningTime { get; set; } = null!;
    public string clubClosingTime { get; set; } = null!;
    public string lastEntryTime { get; set; } = null!;
    public List<ClubReservationTimeDetailModel> clubReservationTimeDetailModel { get; set; } = new List<ClubReservationTimeDetailModel>();
}

public class ClubReservationTimeDetailModel
{
    public string time { get; set; } = null!;
    public string status { get; set; } = null!;
}
public class ResponseStatus
{
    public string? code { get; set; }
    public string? message { get; set; }
}
public class LineGroupAfterReservationModel
{
    public string? flag { get; set; }
    public string? clubId { get; set; }
    public string? noOfPeople { get; set; }
    public string? planId { get; set; }
    public string? userName { get; set; }
    public string? reservationId { get; set; }
    public long selectedDateTime { get; set; }
}