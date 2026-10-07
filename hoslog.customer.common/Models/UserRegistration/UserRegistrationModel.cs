using FluentValidation;
using hoslog.customer.common.Models;
using hoslog.customer.shared;

namespace hoslog.customer.api.Models.UserRegistration
{
    #region Create Account
    public class UserRegistrationModel : CommonDTO
    {
        public string? nickName { get; set; }
        public string? mobileNumber { get; set; }
        public string? dob { get; set; }
        public string? email { get; set; }
        public string? referCode { get; set; }
        public string? referType { get; set; }
        public bool isCheckedTermsAndCondition { get; set; }
    }

    public class UserRegistrationResponseModel
    {
        public string? holdCustomerId { get; set; }
        public string? mobileNumber { get; set; }
        public long expiryTime { get; set; }
        public string? processId { get; set; }
    }
    public class UserRegistrationModelValidator : CommonDTOValidator<UserRegistrationModel>
    {
        public UserRegistrationModelValidator()
        {
            RuleFor(x => x.nickName)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithErrorCode(ResponseCodes.CUEREG105_NicknameIsRequired);
            RuleFor(x => x.mobileNumber)
                .Must(value => !string.IsNullOrEmpty(value))
                .WithErrorCode(ResponseCodes.CUEREG106_MobileNumberIsRequired);
            RuleFor(x => x.dob).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CodeCUEREG117_DOB_is_required);
            RuleFor(x => x.email).Must(value => !string.IsNullOrEmpty(value)).WithErrorCode(ResponseCodes.CodeCUEREG116_Email_Address_is_required)
                .EmailAddress().WithErrorCode(ResponseCodes.CodeCUEREG118_Invalid_Email_Format);
        }
    }

    #endregion
    #region User Registration validation
    public class UserRegistrationValidationModel : CommonDTO
    {
        public string? otpCode { get; set; }
        public string? mobileNumber { get; set; }
        public string? holdCustomerId { get; set; }
        public string? processId { get; set; }
    }
    public class UserRegistrationValidationModelValidator : CommonDTOValidator<UserRegistrationValidationModel>
    {
        public UserRegistrationValidationModelValidator()
        {
            RuleFor(x => x.otpCode)
                .Must(value => !string.IsNullOrEmpty(value))
                .WithErrorCode(ResponseCodes.CE470_OTPCodeRequired);
            RuleFor(x => x.mobileNumber)
                .Must(value => !string.IsNullOrEmpty(value))
                .WithErrorCode(ResponseCodes.CE469_MobileNumberRequired);
            RuleFor(x => x.holdCustomerId)
             .Must(value => !string.IsNullOrEmpty(value))
             .WithErrorCode(ResponseCodes.CUEREG108_HoldCustomerIdRequired);
            RuleFor(x => x.processId)
             .Must(value => !string.IsNullOrEmpty(value))
             .WithErrorCode(ResponseCodes.CE463_ProcessIdRequired);

        }
    }
    public class UserRegistrationVerificationResponseModel
    {
        public string? customerId { get; set; }
        public string? mobileNumber { get; set; }
        public string? processId { get; set; }

    }
    #endregion
    #region CreatePassword
    public class CreatePasswordModel : CommonDTO
    {
        public string? customerId { get; set; }
        public string? mobileNumber { get; set; }
        public string? password { get; set; }
        public string? confirmPassword { get; set; }
        public string? processId { get; set; }
    }
    public class CreatePasswordModelValidator : CommonDTOValidator<CreatePasswordModel>
    {
        public CreatePasswordModelValidator()
        {
            RuleFor(x => x.customerId)
                .Must(value => !string.IsNullOrEmpty(value))
                .WithErrorCode(ResponseCodes.CE465_CustomerIdRequired);
            RuleFor(x => x.mobileNumber)
                .Must(value => !string.IsNullOrEmpty(value))
                .WithErrorCode(ResponseCodes.CUEREG106_MobileNumberIsRequired);
            RuleFor(x => x.password)
             .Must(value => !string.IsNullOrEmpty(value))
             .WithErrorCode(ResponseCodes.CE466_PasswordRequired);
            RuleFor(x => x.confirmPassword)
                .Must(value => !string.IsNullOrEmpty(value))
                .WithErrorCode(ResponseCodes.CE467_ConfirmPasswordRequired);
            RuleFor(x => x.processId)
               .Must(value => !string.IsNullOrEmpty(value))
               .WithErrorCode(ResponseCodes.CE463_ProcessIdRequired);
        }
    }
    public class CreatePasswordResponseModel
    {
        public string? customerId { get; set; }
        public string? mobileNumber { get; set; }
    }
    #endregion
    #region Resend OTP
    public class ResendOTPRequestModel : CommonDTO
    {
        public string? holdCustomerId { get; set; }
        public string? mobileNumber { get; set; }
    }
    public class ResendOTPRequestModelValidator : CommonDTOValidator<ResendOTPRequestModel>
    {
        public ResendOTPRequestModelValidator()
        {
            RuleFor(x => x.holdCustomerId)
                .Must(value => !string.IsNullOrEmpty(value))
                .WithErrorCode(ResponseCodes.CUEREG108_HoldCustomerIdRequired);
            RuleFor(x => x.mobileNumber)
                .Must(value => !string.IsNullOrEmpty(value))
                .WithErrorCode(ResponseCodes.CE469_MobileNumberRequired);
        }
    }
    public class ResendOTPResponseModel
    {
        public string? holdCustomerId { get; set; }
        public string? mobileNumber { get; set; }
        public string? processId { get; set; }
        public long? expiryTime { get; set; }
    }

    #endregion
    #region Forgot Password
    public class ForgotPasswordRequestModel : CommonDTO
    {
        public string? mobileNumber { get; set; }
    }
    public class ForgotPasswordRequestModelValidator : CommonDTOValidator<ForgotPasswordRequestModel>
    {
        public ForgotPasswordRequestModelValidator()
        {
            RuleFor(x => x.mobileNumber)
                .Must(value => !string.IsNullOrEmpty(value))
                .WithErrorCode(ResponseCodes.CE469_MobileNumberRequired);
        }
    }
    public class ForgotPasswordResponseModel
    {
        public string? customerId { get; set; }
        public string? mobileNumber { get; set; }
        public long? expiryTime { get; set; }
        public string? processId { get; set; }
    }

    public class ForgotPasswordOTPVerificatonRequestModel : CommonDTO
    {
        public string? customerId { get; set; }
        public string? otpCode { get; set; }
        public string? mobileNumber { get; set; }
        public string? processId { get; set; }
    }
    public class ForgotPasswordOTPVerificatonRequestModelValidator : CommonDTOValidator<ForgotPasswordOTPVerificatonRequestModel>
    {
        public ForgotPasswordOTPVerificatonRequestModelValidator()
        {
            RuleFor(x => x.customerId)
                .Must(value => !string.IsNullOrEmpty(value))
                .WithErrorCode(ResponseCodes.CE465_CustomerIdRequired);
            RuleFor(x => x.otpCode)
              .Must(value => !string.IsNullOrEmpty(value))
              .WithErrorCode(ResponseCodes.CE470_OTPCodeRequired);
            RuleFor(x => x.mobileNumber)
             .Must(value => !string.IsNullOrEmpty(value))
             .WithErrorCode(ResponseCodes.CE469_MobileNumberRequired);
            RuleFor(x => x.processId)
             .Must(value => !string.IsNullOrEmpty(value))
             .WithErrorCode(ResponseCodes.CE463_ProcessIdRequired);
        }
    }
    public class ForgotPasswordVerificationResponseModel
    {
        //public string userId { get; set; }
        public string? customerId { get; set; }
        public string? mobileNumber { get; set; }
    }

    public class ForgotPasswordResendOTPModel : CommonDTO
    {
        public string? customerId { get; set; }
        public string? mobileNumber { get; set; }
    }
    public class ForgotPasswordResendOTPModelValidator : CommonDTOValidator<ForgotPasswordResendOTPModel>
    {
        public ForgotPasswordResendOTPModelValidator()
        {
            RuleFor(x => x.customerId)
                .Must(value => !string.IsNullOrEmpty(value))
                .WithErrorCode(ResponseCodes.CE465_CustomerIdRequired);
            RuleFor(x => x.mobileNumber)
              .Must(value => !string.IsNullOrEmpty(value))
              .WithErrorCode(ResponseCodes.CE469_MobileNumberRequired);

        }
    }
    public class ForgotPasswordResendResponseModel
    {
        public string? mobileNumber { get; set; }
        public string? customerId { get; set; }
        public string? processId { get; set; }
        public long expiryTime { get; set; }
    }
    #endregion
    #region changePassword
    public class ChangeCustomerPasswordRequestModel : CommonDTO
    {
        public string? oldPassword { get; set; }
        public string? newPassword { get; set; }
        public string? confirmPassword { get; set; }
    }
    public class ChangeCustomerPasswordRequestModelValidator : CommonDTOValidator<ChangeCustomerPasswordRequestModel>
    {
        public ChangeCustomerPasswordRequestModelValidator()
        {
            RuleFor(x => x.oldPassword)
                .Must(value => !string.IsNullOrEmpty(value))
                .WithErrorCode(ResponseCodes.CUEREG114_OldPasswordRequired);
            RuleFor(x => x.newPassword)
              .Must(value => !string.IsNullOrEmpty(value))
              .WithErrorCode(ResponseCodes.CUEREG115_NewPasswordRequired);
            RuleFor(x => x.confirmPassword)
              .Must(value => !string.IsNullOrEmpty(value))
              .WithErrorCode(ResponseCodes.CE467_ConfirmPasswordRequired);
        }
    }
    public class ChangeCustomerPasswordResponseModel
    {
        public string? customerId { get; set; }
        //public string userId { get; set; }   
    }
    public class ForgotSetPasswordModel : CommonDTO
    {
        public string? customerId { get; set; }
        public string? password { get; set; }
        public string? confirmPassword { get; set; }
        public string? processId { get; set; }
    }
    public class ForgotSetPasswordModelValidator : CommonDTOValidator<ForgotSetPasswordModel>
    {
        public ForgotSetPasswordModelValidator()
        {
            RuleFor(x => x.customerId)
                .Must(value => !string.IsNullOrEmpty(value))
                .WithErrorCode(ResponseCodes.CE465_CustomerIdRequired);
            RuleFor(x => x.password)
              .Must(value => !string.IsNullOrEmpty(value))
              .WithErrorCode(ResponseCodes.CE466_PasswordRequired);
            RuleFor(x => x.confirmPassword)
              .Must(value => !string.IsNullOrEmpty(value))
              .WithErrorCode(ResponseCodes.CE467_ConfirmPasswordRequired);
            RuleFor(x => x.processId)
              .Must(value => !string.IsNullOrEmpty(value))
              .WithErrorCode(ResponseCodes.CE463_ProcessIdRequired);
        }
    }
    #endregion
}
