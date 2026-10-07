using FluentValidation;
using hoslog.customer.shared;

namespace hoslog.customer.common.Models
{
    public class CommonDTO
    {
        public string? actionPlatform { get; set; }
    }

    public class PaginationRequestModel
    {
        public int? pageNo { get; set; }
    }
    
    public class CommonDTOValidator<T> : AbstractValidator<T> where T : CommonDTO
    {
        public CommonDTOValidator()
        {
            RuleFor(x => x.actionPlatform)
                .NotNull().NotEmpty().WithErrorCode(ResponseCodes.CE475_ActionPlatformRequired);
        }
    }
}
