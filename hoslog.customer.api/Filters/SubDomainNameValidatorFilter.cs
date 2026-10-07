// using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
// using hoslog.customer.business.Multitenant;
// using hoslog.customer.common.Service;
// using hoslog.customer.shared;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.Filters;

// namespace hoslog.customer.api.Filters
// {
//     public class SubDomainNameValidatorFilter : IAsyncActionFilter
//     {
//         private readonly IClubInformationMultitenantBusiness _clubInformationBusiness;
//         private readonly ApiResponseMessageService _apiResponseMessageService;

//         public SubDomainNameValidatorFilter(
//             IClubInformationMultitenantBusiness clubInformationBusiness,
//             ApiResponseMessageService apiResponseMessageService)
//         {
//             _clubInformationBusiness = clubInformationBusiness;
//             _apiResponseMessageService = apiResponseMessageService;
//         }

//         public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
//         {
//             var headerValue = context.HttpContext.Items["RequestSourceUrl"] as string;

//             if (string.IsNullOrEmpty(headerValue))
//             {
//                 var originHeader = context.HttpContext.Request.Headers["Origin"].FirstOrDefault();
//                 var refererHeader = context.HttpContext.Request.Headers["Referer"].FirstOrDefault();
//                 headerValue = originHeader ?? refererHeader;
//             }

//             if (string.IsNullOrEmpty(headerValue))
//             {
//                 var error = new CommonApiResponse
//                 {
//                     code = ResponseCodes.Code400_BadRequest,
//                     message = await _apiResponseMessageService.GetMessageByCodeAsync(ResponseMessages.Msg400_BadRequest)
//                 };
//                 context.Result = new BadRequestObjectResult(error);
//                 return;
//             }

//             var dbResponse = await _clubInformationBusiness.ValidateClubSubDomain(ExtractSubdomain(headerValue));
//             if (dbResponse.code != "0")
//             {
//                 var error = new CommonApiResponse
//                 {
//                     code = ResponseCodes.Code400_BadRequest,
//                     message = await _apiResponseMessageService.GetMessageByCodeAsync(dbResponse?.message ?? ResponseMessages.Msg400_BadRequest)
//                 };
//                 context.Result = new BadRequestObjectResult(error);
//                 return;
//             }

//             await next(); // continue to controller
//         }

//         private string ExtractSubdomain(string url)
//         {
//             // First try to parse with proper scheme
//             Uri uri = null;

//             if (Uri.TryCreate(url, UriKind.Absolute, out uri) &&
//                 (uri.Scheme == "http" || uri.Scheme == "https"))
//             {
//                 // Already has proper scheme
//             }
//             else
//             {
//                 // Try adding scheme
//                 if (!Uri.TryCreate("https://" + url, UriKind.Absolute, out uri))
//                 {
//                     if (!Uri.TryCreate("http://" + url, UriKind.Absolute, out uri))
//                     {
//                         return string.Empty;
//                     }
//                 }
//             }

//             var parts = uri.Host.Split('.');

//             // For localhost: cardie.localhost -> return "cardie"
//             if (parts.Length >= 2 && parts[parts.Length - 1] == "localhost")
//             {
//                 return parts[0];
//             }

//             // For regular domains: sub.domain.com -> return "sub"
//             if (parts.Length > 2)
//             {
//                 return parts[0];
//             }

//             return string.Empty;
//         }
//     }
// }