using hoslog.customer.api.Models;
using hoslog.customer.business.Multitenant;
using hoslog.customer.common.Service;
using hoslog.customer.shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;

namespace hoslog.customer.api.Filters
{
    public class SubDomainNameValidatorFilterV2 : IAsyncActionFilter
    {
        private readonly IClubInformationMultitenantBusiness _clubInformationBusiness;
        private readonly ApiResponseMessageService _apiResponseMessageService;

        public SubDomainNameValidatorFilterV2(
            IClubInformationMultitenantBusiness clubInformationBusiness,
            ApiResponseMessageService apiResponseMessageService)
        {
            _clubInformationBusiness = clubInformationBusiness;
            _apiResponseMessageService = apiResponseMessageService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var sourceUri =
                context.HttpContext.Items[
                    RequestSourceContextKeys.sourceUri] as Uri;

            var subdomain =
                context.HttpContext.Items[
                    RequestSourceContextKeys.subdomain] as string;

            if (sourceUri is null)
            {
                await SetBadRequestAsync(context);
                return;
            }

            if (string.IsNullOrWhiteSpace(subdomain))
            {
                await next();
                return;
            }

            var referralUrlInfo = ExtractReferralUrlInfo(sourceUri, subdomain);

            var dbResponse =
            await _clubInformationBusiness.ValidateClubSubDomain(
                referralUrlInfo.subdomain,
                referralUrlInfo.clubCode
            );

            if (dbResponse is null || dbResponse.code != "0")
            {
                await SetBadRequestAsync(
                    context,
                    dbResponse?.message);

                return;
            }

            await next();
        }

        private static ReferralUrlInfo ExtractReferralUrlInfo(
            Uri sourceUri,
            string subdomain)
        {
            var pathSegments = sourceUri.AbsolutePath.Split(
                '/',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries
            );

            var clubCode = pathSegments.FirstOrDefault()
                ?? string.Empty;

            clubCode = Uri.UnescapeDataString(clubCode);

            var queryParameters =
                QueryHelpers.ParseQuery(sourceUri.Query);

            var referralType =
                queryParameters.TryGetValue(
                    "type",
                    out var typeValue)
                    ? typeValue.FirstOrDefault() ?? string.Empty
                    : string.Empty;

            return new ReferralUrlInfo
            {
                subdomain = subdomain,
                clubCode = clubCode,
                referralType = referralType
            };
        }

        private async Task SetBadRequestAsync(
            ActionExecutingContext context,
            string? messageCode = null)
        {
            var resolvedMessageCode =
                string.IsNullOrWhiteSpace(messageCode)
                    ? ResponseMessages.Msg400_BadRequest
                    : messageCode;

            context.Result = new BadRequestObjectResult(
                new CommonApiResponse
                {
                    code = ResponseCodes.Code400_BadRequest,
                    message = await _apiResponseMessageService
                        .GetMessageByCodeAsync(resolvedMessageCode)
                }
            );
        }

        private sealed class ReferralUrlInfo
        {
            public string subdomain { get; init; } = string.Empty;
            public string clubCode { get; init; } = string.Empty;
            public string referralType { get; init; } = string.Empty;
        }
    }
}