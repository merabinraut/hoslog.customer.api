using FluentValidation.Results;
using hoslog.customer.common.Service;
using hoslog.customer.common.Utilities;
using hoslog.customer.shared;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace hoslog.customer.common
{
    public class ValidationHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly StringCipher _stringCipher;
        private readonly ApiResponseMessageService _apiResponseMessageService;

        public ValidationHelper(IHttpContextAccessor httpContextAccessor, StringCipher stringCipher, ApiResponseMessageService apiResponseMessageService)
        {
            _httpContextAccessor = httpContextAccessor;
            _stringCipher = stringCipher;
            _apiResponseMessageService = apiResponseMessageService;
        }

        public async Task<(HttpStatusCode, object?)> ValidateSignatureAsync<T>(T request)
        {
            var context = _httpContextAccessor.HttpContext;

            if (context?.Request.Headers.TryGetValue("Signature", out var signature) != true)
                return await CreateErrorResponse(ResponseCodes.CodeE601_InvalidSignatureToken);

            _stringCipher.Signature = signature.ToString();

            if (string.IsNullOrEmpty(_stringCipher.Signature) || !_stringCipher.VerifySign(request))
                return await CreateErrorResponse(ResponseCodes.CodeE601_InvalidSignatureToken);

            return (HttpStatusCode.OK, null);
        }

        public async Task<(HttpStatusCode, object?)> HandleModelValidationAsync<T>(
            T request,
            Func<T, ValidationResult> validateRequest)
        {
            var validationResult = validateRequest(request);

            if (!validationResult.IsValid)
            {
                var validationResponse = await GenerateModelValidationResponse(validationResult);
                return (validationResponse.statusCode, validationResponse.response);
            }

            return (HttpStatusCode.OK, null);
        }

        public async Task<(HttpStatusCode, object?)> ValidateModelAndSignatureAsync<T>(T request, Func<T, ValidationResult> validateRequest)
        {
            var (modelValidationSatus, modelValidationResponse) = await HandleModelValidationAsync(request, validateRequest);
            if (modelValidationSatus != HttpStatusCode.OK)
                return (modelValidationSatus, modelValidationResponse);

            var (signatureValidationStatus, signatureValidationResponse) = await ValidateSignatureAsync(request);
            if (signatureValidationStatus != HttpStatusCode.OK)
                return (signatureValidationStatus, signatureValidationResponse);

            return (HttpStatusCode.OK, null);
        }

        private async Task<(HttpStatusCode, object)> CreateErrorResponse(string errorCode)
        {
            return (HttpStatusCode.BadRequest, new CommonApiResponse
            {
                code = errorCode,
                message = await _apiResponseMessageService.GetMessageByCodeAsync(errorCode)
            });
        }

        private async Task<(HttpStatusCode statusCode, CommonApiResponse response)> GenerateModelValidationResponse(ValidationResult request)
        {
            var response = new CommonApiResponse
            {
                code = ResponseCodes.Code400_BadRequest,
                message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseCodes.Code400_BadRequest),
                errors = new List<Errors>()
            };

            foreach (var error in request.Errors)
            {
                var errorMessage = await _apiResponseMessageService.GetMessageByCodeAsync(error.ErrorCode);
                response.errors.Add(new Errors
                {
                    errorCode = error.ErrorCode,
                    errorMessage = errorMessage
                });
            }

            return (HttpStatusCode.BadRequest, response);
        }
    }
}
