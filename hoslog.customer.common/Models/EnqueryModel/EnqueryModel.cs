using FluentValidation;
using hoslog.customer.api.Models.UserRegistration;
using hoslog.customer.shared;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hoslog.customer.common.Models.EnqueryModel
{
    public class InquiriesRequestModel :CommonDTO
    {

        public string? name { get; set; }
        public string? emailAddress { get; set; }
        public string? mobileNumber { get; set; }
        public string? inquiryType { get; set; }
        public string? subject { get; set; }
        public string? description { get; set; }
        public IFormFile? attachment { get; set; }
        public string? companyName { get; set; }
    }
    public class InquiriesRequestModelValidator : CommonDTOValidator<InquiriesRequestModel>
    {
        public InquiriesRequestModelValidator()
        {
            RuleFor(x => x.name)
                .Must(value => !string.IsNullOrWhiteSpace(value))
                .WithErrorCode(ResponseCodes.CodeCUEAUT100_Username_is_required);
            RuleFor(x => x.mobileNumber)
                .Must(value => !string.IsNullOrEmpty(value))
                .WithErrorCode(ResponseCodes.CUEREG106_MobileNumberIsRequired);
            RuleFor(x => x.emailAddress)
                .Must(value => !string.IsNullOrEmpty(value))
                .WithErrorCode(ResponseCodes.CodeCUEREG116_Email_Address_is_required);
            RuleFor(x => x.inquiryType)
              .Must(value => !string.IsNullOrEmpty(value))
              .WithErrorCode(ResponseCodes.CUEEN100_enqueryTypeIsRequired);
            //RuleFor(x => x.description)
            //.Must(value => !string.IsNullOrEmpty(value))
            //.WithErrorCode(ResponseCodes.CUEEN101_descriptionIsRequired);
        }
    }
}
