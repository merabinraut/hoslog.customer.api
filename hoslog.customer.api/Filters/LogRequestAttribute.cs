using hoslog.customer.api.Library;
using hoslog.customer.business.ApiLog;
using hoslog.customer.shared.ApiLog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace hoslog.customer.api.Filters
{
    public class LogRequestAttribute : TypeFilterAttribute
    {
        public LogRequestAttribute() : base(typeof(LogRequestFilter)) { }

        public class LogRequestFilter : IAsyncResourceFilter
        {
            private readonly IApiLogBusiness _apiLogBusiness;

            public LogRequestFilter(IApiLogBusiness apiLogBusiness)
            {
                _apiLogBusiness = apiLogBusiness;
            }

            public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
            {
                await LogRequestAsync(context);
                await next();
            }

            private async Task LogRequestAsync(ResourceExecutingContext context)
            {
                var requestUrl = context.HttpContext.GetRequestUrl();
                var requestIp = context?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                var (_, requestBody) = await context.HttpContext.GetRequestBodyAsStringAsync();
                var reqHeaders = context.HttpContext.GetRequestHeadersAsDictionaryJson();
                var functionName = context.HttpContext.GetAction();
                // var traceId = context.HttpContext.TraceIdentifier;
                var traceId = Guid.NewGuid().ToString();
                context.HttpContext.Items["TraceId"] = traceId;
                ApiLogCommon apiLogCommon = new ApiLogCommon()
                {
                    TraceId = traceId,
                    FunctionName = functionName,
                    RequestUrl = requestUrl,
                    RequestPayload = requestBody,
                    RequestHeaders = reqHeaders,
                    IPAddress = requestIp,
                };

                await _apiLogBusiness.InsertLog(apiLogCommon);
            }
        }
    }
}
