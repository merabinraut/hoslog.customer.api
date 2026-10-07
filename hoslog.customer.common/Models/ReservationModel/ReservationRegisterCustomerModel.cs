using FluentValidation;
using hoslog.customer.shared;

namespace hoslog.customer.common.Models.ReservationModel;

public class ReservationRegisterCustomerRequestModel : CommonDTO
{
    public string? nickName { get; set; }
    public string? emailAddress { get; set; }
    public string? mobileNumber { get; set; }
    public string? dob { get; set; }
    public string? reservationHoldId { get; set; }
}

public class ReservationRegisterCustomerRequestValidatorModel : CommonDTOValidator<ReservationRegisterCustomerRequestModel>
{
    public ReservationRegisterCustomerRequestValidatorModel()
    {
        RuleFor(x => x.nickName).Must(value => !string.IsNullOrWhiteSpace(value)).WithErrorCode(ResponseCodes.CE468_NicknameRequired);
        RuleFor(x => x.emailAddress).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CodeCUEREG116_Email_Address_is_required)
            .EmailAddress().WithErrorCode(ResponseCodes.CodeCUEREG118_Invalid_Email_Format);
        RuleFor(x => x.dob).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CodeCUEREG117_DOB_is_required);
        RuleFor(x => x.mobileNumber).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CE469_MobileNumberRequired);
    }
}