using hoslog.customer.api.Library;
using hoslog.customer.business.ExceptionLog;
using hoslog.customer.shared;
using System.Net;
using System.Text.Json;

namespace hoslog.customer.api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, IHostEnvironment env, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _env = env;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext, IExceptionLogBusiness exceptionLogger)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong: {ex}");

                var isClientError = httpContext.Response.StatusCode is >= 400 and < 500;
                var statusCode = isClientError ? httpContext.Response.StatusCode : (int)HttpStatusCode.InternalServerError;

                var response = _env.IsDevelopment()
                    ? isClientError
                        ? new CommonApiResponse
                        {
                            code = statusCode.ToString(),
                            //ResponseStatus = ResponseStatuses.Error 
                        }
                        : new CommonApiResponse
                        {
                            code = statusCode.ToString(),
                            //ResponseStatus = ResponseStatuses.Error,
                            message = ex.Message,
                            //ResponseDetailMessage = ex.StackTrace
                        }
                    : isClientError
                        ? new CommonApiResponse
                        {
                            code = statusCode.ToString(),
                            //ResponseStatus = ResponseStatuses.Error 
                        }
                        : new CommonApiResponse
                        {
                            code = statusCode.ToString(),
                            //ResponseStatus = ResponseStatuses.Error 
                        };
                var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

                var jsonResponse = JsonSerializer.Serialize(response, response.GetType(), jsonSerializerOptions);

                if (!httpContext.Request.Path.StartsWithSegments("/swagger") && statusCode != 404)
                {
                    //await Task.WhenAll(
                    //exceptionLogger.WriteExceptionLog(ex)
                    // When exception occurs, 'response log' filter cannot update the log, that's why we log here
                    //vendorApiLogger.LogUpdateResponseAsync(responseHttpStatus: statusCode, responseOutput: jsonResponse)
                    //);
                    var ip = httpContext?.Connection?.RemoteIpAddress?.ToString();
                    var url = httpContext.GetRequestUrl();

                    exceptionLogger.WriteExceptionLog(ex, ip, url);
                }

                if (!httpContext.Response.HasStarted)
                {
                    httpContext.Response.ContentType = "application/json";
                    httpContext.Response.StatusCode = statusCode;
                    await httpContext.Response.WriteAsync(jsonResponse);
                }

                //await HandleExceptionAsync(httpContext, ex);
            }
        }
    }
}