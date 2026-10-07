using FluentValidation;
using hoslog.customer.shared;

namespace hoslog.customer.common.Models.User
{
    public class ProfileModel : CommonDTO
    {
        public string profileImage { get; set; }
        //public IFormFile? profileImage { get; set; }
    }
    public class ProfileImageRequestModelValidator : CommonDTOValidator<ProfileModel>
    {
        public ProfileImageRequestModelValidator()
        {
            RuleFor(x => x.profileImage)
            .NotNull()
            .WithErrorCode(ResponseCodes.CodeCUE117_Profile_image_is_required);
            //.WithMessage("Profile image is required.");

            // Optional: Validate the file size or file type
            RuleFor(x => x.profileImage)
                .Must(file => file == null || (file.Length > 0 && file.Length <= 5 * 1024 * 1024)) // Example for a max file size of 5MB
                .WithErrorCode(ResponseCodes.CodeCUE118_Profile_image_must_be_less_than_5MB);
        }
    }
}
