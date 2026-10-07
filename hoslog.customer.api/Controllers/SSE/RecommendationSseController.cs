using Asp.Versioning;
using hoslog.customer.common.RealTime;
using hoslog.customer.common.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace hoslog.customer.api.Controllers.SSE
{
    [AllowAnonymous]
    [Route("api/v{version:apiVersion}/sse/recommendation-group")]
    [ApiVersion("1.0")]
    public class RecommendationSseController : BaseApiController
    {
        private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(20);

        private readonly SseHub _sseHub;
        private readonly ILogger<RecommendationSseController> _logger;
        private readonly StringCipher _stringCipher;

        public RecommendationSseController(SseHub sseHub, ILogger<RecommendationSseController> logger, StringCipher stringCipher)
        {
            _sseHub = sseHub;
            _logger = logger;
            _stringCipher = stringCipher;
        }

        [HttpGet("stream")]
        public async Task Stream([FromQuery] string locationId, CancellationToken cancellationToken = default)
        {
            if(!TryDecryptLong(locationId, out long decryptLocationId) || decryptLocationId <= 0)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;

                await Response.WriteAsJsonAsync(
                    new
                    {
                        code = "400",
                        message = "Invalid location ID.",
                        data = (object?)null,
                        errors = Array.Empty<string>()
                    },
                    cancellationToken);

                return;
            }
            var topic = SseTopicNames.RecommendationGroupLocation(decryptLocationId);

            HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();

            Response.StatusCode = StatusCodes.Status200OK;
            Response.ContentType = "text/event-stream";
            Response.Headers.CacheControl = "no-cache, no-transform";
            Response.Headers.Append("X-Accel-Buffering", "no");

            var subscription = _sseHub.Subscribe(topic);

            try
            {
                await Response.WriteAsync("retry: 5000\n", cancellationToken);
                await Response.WriteAsync(": connected\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);

                while (!cancellationToken.IsCancellationRequested)
                {
                    var waitForMessageTask = subscription.reader.WaitToReadAsync(cancellationToken).AsTask();

                    var heartbeatTask = Task.Delay(HeartbeatInterval, cancellationToken);
                    var completedTask = await Task.WhenAny(waitForMessageTask, heartbeatTask);

                    if(completedTask == waitForMessageTask)
                    {
                        var canRead = await waitForMessageTask;

                        if (!canRead)
                            break;

                        while(subscription.reader.TryRead(out var envelope))
                        {
                            await SseResponseWriter.WriteEventAsync(Response, envelope, cancellationToken);
                        }
                        continue;
                    }

                    await Response.WriteAsync($": heartbeat {DateTime.UtcNow:O}\n\n", cancellationToken);

                    await Response.Body.FlushAsync(cancellationToken);
                }
            }
            catch(OperationCanceledException)
            {
                _logger.LogInformation("Recommendation group SSE client disconnected. LocationId: {locationId}", decryptLocationId);
            }
            finally
            {
                _sseHub.Unsubscribe(topic, subscription.connectionId);
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
                return false;
            }
        }

    }
}
