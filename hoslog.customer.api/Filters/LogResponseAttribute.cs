using hoslog.customer.api.Library;
using hoslog.customer.business.ApiLog;
using hoslog.customer.shared.ApiLog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace hoslog.customer.api.Filters
{
    public class LogResponseAttribute : TypeFilterAttribute
    {
        public LogResponseAttribute() : base(typeof(LogResponseFilter)) { }

        public class LogResponseFilter : IAsyncResourceFilter
        {
            private readonly IApiLogBusiness _apiLogBusiness;


            public LogResponseFilter(IApiLogBusiness apiLogBusiness)
            {
                _apiLogBusiness = apiLogBusiness;
            }

            public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
            {

                // Before the action method is executed
                var originalResponseStream = context.HttpContext.Response.Body;
                var memoryStream = new MemoryStream();
                context.HttpContext.Response.Body = memoryStream;

                var executedContext = await next();
                var traceId = executedContext.HttpContext.Items["TraceId"] as string ?? executedContext.HttpContext.TraceIdentifier;
                var functionName = executedContext.HttpContext.GetAction();
                var requestIp = context?.HttpContext?.Connection?.RemoteIpAddress?.ToString();

                memoryStream.Seek(0, SeekOrigin.Begin);
                var responseBody = new StreamReader(memoryStream).ReadToEnd();
                memoryStream.Seek(0, SeekOrigin.Begin);

                context.HttpContext.Response.Body = originalResponseStream;
                await memoryStream.CopyToAsync(originalResponseStream);

                _apiLogBusiness.UpdateLog(new ApiLogCommon()
                {
                    // TraceId = executedContext.HttpContext.TraceIdentifier,
                    TraceId = traceId,
                    FunctionName = functionName,
                    ResponseHttpStatus = executedContext.HttpContext.Response.StatusCode.ToString(),
                    ResponsePayload = responseBody,
                    IPAddress = requestIp
                });
                //await _vendorApiLogger.LogUpdateResponseAsync(responseHttpStatus: executedContext.HttpContext.Response.StatusCode, responseOutput: responseBody);
            }

            private string[] ResponseContentTypes => new string[] { "text/plain", "text/html", "application/xml", "text/xml", "application/json", "text/json" };
        }
    }
}
