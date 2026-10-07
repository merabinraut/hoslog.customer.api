using Asp.Versioning;
using hoslog.customer.common.RealTime;
using hoslog.customer.common.Utilities;
using Hoslog.Customer.Common.RealTime.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace hoslog.customer.api.Controllers.SSE
{
    [Route("api/v{version:apiVersion}/sse/visit-now")]
    [ApiVersion("1.0")]
    public class VisitNowSseController : BaseApiController
    {
        private static readonly TimeSpan HeartbeatInterval =
            TimeSpan.FromSeconds(20);

        private readonly SseHub _sseHub;
        private readonly ILogger<VisitNowSseController> _logger;
        private readonly StringCipher _stringCipher;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VisitNowSseController(
            SseHub sseHub,
            ILogger<VisitNowSseController> logger,
            StringCipher stringCipher,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _sseHub = sseHub;
            _logger = logger;
            _stringCipher = stringCipher;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("stream")]
        public async Task Stream(
            [FromQuery] string locationId,
            CancellationToken cancellationToken = default)
        {
            if (!TryDecryptLong(locationId, out long decryptedLocationId)
                || decryptedLocationId <= 0)
            {
                _logger.LogWarning(
                    "SSE connection rejected because location ID is invalid."
                );

                Response.StatusCode = StatusCodes.Status400BadRequest;

                await Response.WriteAsJsonAsync(
                    new
                    {
                        code = "400",
                        message = "Invalid location ID.",
                        data = (object?)null,
                        errors = Array.Empty<string>()
                    },
                    cancellationToken
                );
            }

            string timeZoneOffsetValue = _httpContextAccessor?.HttpContext?.Items?["TimezoneOffset"]?.ToString();

            if
            (
                !TimeZoneOffsetHelper.TryNormalize(
                timeZoneOffsetValue,
                out var normalizedTimeZoneOffset)
            )
            {
                Response.StatusCode =
                        StatusCodes.Status400BadRequest;

                await Response.WriteAsJsonAsync(
                    new
                    {
                        code = "400",
                        message = "Invalid timezone offset.",
                        data = (object?)null,
                        errors = Array.Empty<string>()
                    },
                    cancellationToken);

                return;
            }

            var topic = SseTopicNames.VisitNowLocation(
                decryptedLocationId,
                normalizedTimeZoneOffset
            );

            HttpContext.Features
                .Get<IHttpResponseBodyFeature>()
                ?.DisableBuffering();

            Response.StatusCode = StatusCodes.Status200OK;
            Response.ContentType = "text/event-stream";
            Response.Headers.CacheControl = "no-cache, no-transform";
            Response.Headers.Append("X-Accel-Buffering", "no");

            var subscription = _sseHub.Subscribe(topic);

            _logger.LogInformation(
                "SSE client connected. LocationId: {LocationId}, ConnectionId: {ConnectionId}",
                decryptedLocationId,
                subscription.connectionId);

            try
            {
                await Response.WriteAsync(
                    "retry: 5000\n",
                    cancellationToken);

                await Response.WriteAsync(
                    ": connected\n\n",
                    cancellationToken);

                await Response.Body.FlushAsync(cancellationToken);

                await ProcessStreamAsync(
                    subscription.reader,
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation(
                    "SSE client disconnected. LocationId: {LocationId}, ConnectionId: {ConnectionId}",
                    decryptedLocationId,
                    subscription.connectionId);
            }
            catch (IOException ex)
            {
                _logger.LogInformation(
                   ex,
                   "SSE network connection closed. LocationId: {LocationId}, ConnectionId: {ConnectionId}",
                   decryptedLocationId,
                   subscription.connectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                   ex,
                   "SSE stream failed. LocationId: {LocationId}, ConnectionId: {ConnectionId}",
                   decryptedLocationId,
                   subscription.connectionId);
            }
            finally
            {
                _sseHub.Unsubscribe(topic, subscription.connectionId);

                _logger.LogInformation(
                  "SSE subscription removed. LocationId: {LocationId}, ConnectionId: {ConnectionId}",
                  decryptedLocationId,
                  subscription.connectionId);
            }

            async Task ProcessStreamAsync(
                            System.Threading.Channels.ChannelReader<SseCommandEnvelope> reader,
                            CancellationToken token)
            {
                while (!token.IsCancellationRequested)
                {
                    var waitForMessageTask = reader
                        .WaitToReadAsync(token)
                        .AsTask();

                    var heartbeatTask = Task.Delay(
                        HeartbeatInterval,
                        token);

                    var completedTask = await Task.WhenAny(
                        waitForMessageTask,
                        heartbeatTask);

                    if (completedTask == waitForMessageTask)
                    {
                        var canRead = await waitForMessageTask;

                        if (!canRead)
                        {
                            _logger.LogInformation(
                                "SSE channel completed. LocationId: {LocationId}, ConnectionId: {ConnectionId}",
                                decryptedLocationId,
                                subscription.connectionId);

                            break;
                        }

                        while (reader.TryRead(out var envelope))
                        {
                            await SseResponseWriter.WriteEventAsync(
                                Response,
                                envelope,
                                token);
                        }

                        continue;
                    }

                    await Response.WriteAsync(
                        $": heartbeat {DateTime.UtcNow:O}\n\n",
                        token);

                    await Response.Body.FlushAsync(token);
                }
            }
        }

        private bool TryDecryptLong(string? encryptedValue, out long decryptedId)
        {
            decryptedId = 0;

            if (string.IsNullOrWhiteSpace(encryptedValue))
                return false;

            try
            {
                var decryptedValue = _stringCipher.Decrypt(encryptedValue);
                return long.TryParse(decryptedValue, out decryptedId);
            }
            catch
            {
                decryptedId = 0;
                return false;
            }
        }
    }
}