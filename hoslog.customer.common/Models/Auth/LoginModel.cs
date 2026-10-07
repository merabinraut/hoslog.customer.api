using FluentValidation;
using hoslog.customer.shared;

namespace hoslog.customer.common.Models.Auth
{
    public class LoginRequestModel : CommonDTO
    {
        public string? loginId { get; set; }
        public string? password { get; set; }
    }

    public class LoginRequestModelValidator : CommonDTOValidator<LoginRequestModel>
    {
        public LoginRequestModelValidator()
        {
            RuleFor(x => x.loginId)
                .Must(value => !string.IsNullOrEmpty(value))
                .WithErrorCode(ResponseCodes.CodeCUE115_Login_id_is_required);
            //.WithMessage("Login id is required.");
            //.Matches(@"^[0-9]{11}$").WithMessage("Mobile number must be 11 digits.");
            RuleFor(x => x.password)
               .Must(value => !string.IsNullOrEmpty(value))
               .WithErrorCode(ResponseCodes.CE466_PasswordRequired)
               //.WithMessage("Password is required.")
               .Length(8, 32).WithErrorCode(ResponseCodes.CodeCUE116_Password_must_be_between_8_to_32_characters);
            //.WithMessage("Password must be between 8 to 32 characters.");
        }
    }

    public class UserClaimsName
    {
        public const string Username = "Username";
        public const string UserId = "UserId";
        public const string AgentId = "AgentId";
        public const string Email = "Email";
        public const string MobileNumber = "MobileNumber";
        public const string RoleType = "RoleType";
    }
}
