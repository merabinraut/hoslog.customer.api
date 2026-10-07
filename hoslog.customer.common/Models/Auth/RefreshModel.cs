using FluentValidation;
using hoslog.customer.shared;

namespace hoslog.customer.common.Models.Auth
{
    public class RefreshTokenRequestModel : CommonDTO
    {
        public string? username { get; set; }
        public string? refreshToken { get; set; }
    }
    public class RefreshTokenRequestModelValidator : CommonDTOValidator<RefreshTokenRequestModel>
    {
        public RefreshTokenRequestModelValidator()
        {
            RuleFor(x => x.username)
               .Must(x => !string.IsNullOrEmpty(x))
               .WithErrorCode(ResponseCodes.CodeCUEAUT100_Username_is_required);
               //.WithMessage("Username is required.");
            RuleFor(x => x.refreshToken)
                .Must(token => !string.IsNullOrEmpty(token))
                .WithErrorCode(ResponseCodes.CodeCUEAUT101_Refresh_token_is_required);
                //.WithMessage("Refresh token is required.");
        }
    }
}
