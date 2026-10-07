using FluentValidation;
using hoslog.customer.shared;

namespace hoslog.customer.common.Models.Reservation
{
    public class ReservationModel : CommonDTO
    {
        public string? reservationId { get; set; }
        public string? timeZoneOffsetValue { get; set; }
    }

    public class ReservationCancelModel : CommonDTO
    {
        public string? reservationId { get; set; }
        public string? actionIp { get; set; }
    }

    public class ReservationTimelModel : CommonDTO
    {
        public string? reservationId { get; set; }
        public long? time { get; set; }
        public string? actionIp { get; set; }
    }
    public class ReservationRequestModelValidator : CommonDTOValidator<ReservationModel>
    {
        public ReservationRequestModelValidator()
        {
            RuleFor(x => x.reservationId)
                .Must(value => !string.IsNullOrEmpty(value))
                .WithErrorCode(ResponseCodes.CUERES127_ReservationIdRequired);
        }
    }
    public class ReservationCancelRequestModelValidator : CommonDTOValidator<ReservationCancelModel>
    {
        public ReservationCancelRequestModelValidator()
        {
            RuleFor(x => x.reservationId)
                .Must(value => !string.IsNullOrEmpty(value))
                .WithErrorCode(ResponseCodes.CUERES127_ReservationIdRequired);
            RuleFor(x => x.actionIp)
                .Must(value => !string.IsNullOrEmpty(value))
                .WithErrorCode(ResponseCodes.CE474_ActionIpRequired);
        }
    }

    public class ReservationTimeRequestModelValidator : CommonDTOValidator<ReservationTimelModel>
    {
        public ReservationTimeRequestModelValidator()
        {
            RuleFor(x => x.reservationId)
                .Must(value => !string.IsNullOrEmpty(value))
                .WithErrorCode(ResponseCodes.CUERES127_ReservationIdRequired);
            RuleFor(x => x.time).Must(value => value > 0).WithErrorCode(ResponseCodes.CUERES128_ReservationTimeRequired);
            RuleFor(x => x.actionIp)
                .Must(value => !string.IsNullOrEmpty(value))
                .WithErrorCode(ResponseCodes.CE474_ActionIpRequired);
        }
    }
}
