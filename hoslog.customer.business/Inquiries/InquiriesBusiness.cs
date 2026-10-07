using Amazon.SecurityToken;
using hoslog.customer.api.Models.UserRegistration;
using hoslog.customer.business.CommonHandler;
using hoslog.customer.common;
using hoslog.customer.common.Models.Auth;
using hoslog.customer.common.Models.EnqueryModel;
using hoslog.customer.common.Service;
using hoslog.customer.common.Utilities;
using hoslog.customer.domain.Inquiries;
using hoslog.customer.shared;
using hoslog.customer.shared.Inquiries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Newtonsoft.Json;
using System.Net;
using System.Numerics;

namespace hoslog.customer.business.Inquiries
{
    public class InquiriesBusiness : IInquiriesBusiness
    {
        private readonly IInquiriesRepository _inquiriesRepository;
        private readonly ValidationHelper _validationHelper;
        private readonly ApiResponseMessageService _apiResponseMessageService;
        private readonly StringCipher _stringCipher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CommonAttributeStringCipher _commonAttributeStringCipher;

        public InquiriesBusiness(IInquiriesRepository inquiriesRepository, ValidationHelper validationHelper, ApiResponseMessageService apiResponseMessageService, StringCipher stringCipher, IHttpContextAccessor httpContextAccessor, CommonAttributeStringCipher commonAttributeStringCipher)
        {
            _inquiriesRepository = inquiriesRepository;
            _validationHelper = validationHelper;
            _apiResponseMessageService = apiResponseMessageService;
            _stringCipher = stringCipher;
            _httpContextAccessor = httpContextAccessor;
            _commonAttributeStringCipher = commonAttributeStringCipher;
        }
        public async Task<(HttpStatusCode, object)> PostInquiries(InquiriesRequestModel request, string path = "")
        {
            request.actionPlatform = _httpContextAccessor?.HttpContext?.Request?.Headers["ActionPlatform"] ?? "WEB";

            var resp = new CommonApiResponse();
            var validationResult = await _validationHelper.HandleModelValidationAsync(request, req => new InquiriesRequestModelValidator().Validate(req));
            if (validationResult.Item1 != HttpStatusCode.OK)
                return validationResult;


            var requestMapped = new InquiriesRequestCommon()
            {
                name = request.name,
                emailAddress = request.emailAddress,
                mobileNumber = request.mobileNumber,
                subject = request.subject,
                description = request.description,
                attachment = path,
                companyName = request.companyName
            };

            if (string.IsNullOrEmpty(request.inquiryType))
            {
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeCUEINQ100_Invalid_Inquery_type);
                return (HttpStatusCode.OK, resp);
            }
            requestMapped.inquiryType = Convert.ToInt16(_commonAttributeStringCipher.Decrypt(request.inquiryType));
            if (requestMapped.inquiryType == '5' && string.IsNullOrEmpty(request.companyName))
            {
                resp.code = ResponseCodes.Code400_BadRequest;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.CodeCUEINQ101_company_name_is_required);
                return (HttpStatusCode.OK, resp);
            }
            requestMapped.actionUserId = request.actionPlatform;

            var response = await _inquiriesRepository.PostInquiriesAsync(requestMapped);
            resp.code = ResponseCodes.Code400_BadRequest;
            resp.message = ResponseMessages.Msg400_BadRequest;
            if (response.code == "0")
            {
                resp.code = ResponseCodes.Code200_Success;
                resp.message = await _apiResponseMessageService.GetMessageByCodeAsync(response.message);
                resp.data = Array.Empty<object>();
                return (HttpStatusCode.OK, resp);
            }
            return (HttpStatusCode.BadRequest, resp);
        }
    }
}
