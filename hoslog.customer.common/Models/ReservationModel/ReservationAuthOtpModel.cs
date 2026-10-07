using FluentValidation;
using hoslog.customer.common.Models.Auth;
using hoslog.customer.shared;

namespace hoslog.customer.common.Models.ReservationModel;

public class ReservationVerifyOtpRequestModel : CommonDTO
{
    public string? mobileNumber { get; set; }
    public string? reservationHoldId { get; set; }
    public string? otpCode { get; set; }
}

public class ReservationVerifyOtpRequestValidatorModel : CommonDTOValidator<ReservationVerifyOtpRequestModel>
{
    public ReservationVerifyOtpRequestValidatorModel()
    {
        RuleFor(x => x.mobileNumber).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CE469_MobileNumberRequired);
        RuleFor(x => x.reservationHoldId).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CUERES138_ReservationHoldIdRequired);
        RuleFor(x => x.otpCode).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CE470_OTPCodeRequired);
    }
}

public class ReservationVerifyOtpResponseModel
{
    public bool customerExists { get; set; } = false;
    public string? reservationHoldId { get; set; }
    public TokenPairModel? tokenPair { get; set; }
    public ReservationDetailsModel? reservationData { get; set; }
}