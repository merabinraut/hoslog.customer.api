using System.Diagnostics;
using hoslog.customer.business.ApiLog;
using hoslog.customer.shared.ApiLog;

namespace hoslog.customer.api.Middleware;

public sealed class ApiPerformanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiPerformanceMiddleware> _logger;
    private readonly IApiLogBusiness _apiLogBusiness;

    private const long SlowRequestThresholdMilliseconds = 300; // 1 second
    private const long VerySlowRequestThresholdMilliseconds = 500; // 5 seconds

    public ApiPerformanceMiddleware(
        RequestDelegate next,
        ILogger<ApiPerformanceMiddleware> logger,
        IApiLogBusiness apiLogBusiness
    )
    {
        _next = next;
        _logger = logger;
        _apiLogBusiness = apiLogBusiness;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        context.Response.OnStarting(() =>
        {
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            context.Response.Headers["Server-Timing"] = $"app;dur={elapsedMs:F2}";

            context.Response.Headers["X-Reponse-Time-Ms"] = elapsedMs.ToString("F2");

            return Task.CompletedTask;
        });

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var elapsedMs = stopwatch.ElapsedMilliseconds;

            await LogRequest(
                context,
                elapsedMs);
        }
    }

    private async Task LogRequest(HttpContext context, long elapsedMs)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? "/";
        var statusCode = context.Response.StatusCode;
        var traceId = context.TraceIdentifier;

        if (elapsedMs >= VerySlowRequestThresholdMilliseconds)
        {
            _logger.LogError(
                 "VERY SLOW API | {Method} {Path} | Status: {StatusCode} | Duration: {ElapsedMs} ms | TraceId: {TraceId}",
                 method,
                 path,
                 statusCode,
                 elapsedMs,
                 traceId);

            var logText = $"VERY SLOW API | {method} {path} | Status: {statusCode} | Duration: {elapsedMs} ms | TraceId: {traceId}";

            await _apiLogBusiness.InsertLog(new ApiLogCommon
            {
                TraceId = traceId,
                FunctionName = $"{method} {path}",
                ResponseHttpStatus = statusCode.ToString(),
                ResponsePayload = logText,
                CreatedPlatform = "ApiPerformanceMiddleware"
            });
        }

        if (elapsedMs >= SlowRequestThresholdMilliseconds)
        {
            _logger.LogWarning(
                "SLOW API | {Method} {Path} | Status: {StatusCode} | Duration: {ElapsedMs} ms | TraceId: {TraceId}",
                method,
                path,
                statusCode,
                elapsedMs,
                traceId);
        }
    }
}