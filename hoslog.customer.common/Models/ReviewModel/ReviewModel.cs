using FluentValidation;
using hoslog.customer.shared;

namespace hoslog.customer.common.Models.ReviewModel;

public class ManageReviewRequestModel : CommonDTO
{
    public string? userId { get; set; }
    public string? reservationId { get; set; }
    public string? clubId { get; set; }
    public string[]? hostIdList { get; set; }
    public string? mvpHostId { get; set; }
    public string[]? dichotomousQAIdList { get; set; }
    public string[]? remarkIdList { get; set; }
    public int ratingScale { get; set; }
    public string? intent { get; set; }
    public string? remark { get; set; }
}
public class ManageReviewRequestValidatorModel : CommonDTOValidator<ManageReviewRequestModel>
{
    public ManageReviewRequestValidatorModel()
    {
        RuleFor(x => x.reservationId).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CUERES127_ReservationIdRequired);
        RuleFor(x => x.clubId).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CodeCUE106_Club_Id_is_required);
        RuleFor(x => x.hostIdList).Must(value => value != null && value.Length > 0).WithErrorCode(ResponseCodes.CUERES123_HostListRequired);
        RuleFor(x => x.mvpHostId).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CUERES122_MVPHostIdRequired);
        RuleFor(x => x.dichotomousQAIdList).Must(value => value != null && value.Length > 0).WithErrorCode(ResponseCodes.CUERES126_DichotomousquestionAndAnswerIdListRequired);
        RuleFor(x => x.remarkIdList).Must(value => value != null && value.Length > 0).WithErrorCode(ResponseCodes.CUERES125_ReviewIdListIsRequired);
        RuleFor(x => x.ratingScale).InclusiveBetween(1, 5).WithErrorCode(ResponseCodes.CUERES124_RatingScaleRequirement);
    }
}

