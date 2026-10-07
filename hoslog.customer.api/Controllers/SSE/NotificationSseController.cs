using System.Threading.Channels;
using Asp.Versioning;
using hoslog.customer.common.Models.Auth;
using hoslog.customer.common.RealTime;
using hoslog.customer.common.Utilities;
using Hoslog.Customer.Common.RealTime.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace hoslog.customer.api.Controllers.SSE
{
    [Route("api/v{version:apiVersion}/sse/notification")]
    [ApiVersion("1.0")]
    [Authorize]
    public class NotificationSseController : BaseApiController
    {
        private readonly SseHub _sseHub;
        private readonly ILogger<NotificationSseController> _logger;
        private readonly StringCipher _stringCipher;
        public NotificationSseController(SseHub sseHub, ILogger<NotificationSseController> logger, StringCipher stringCipher)
        {
            _sseHub = sseHub;
            _logger = logger;
            _stringCipher = stringCipher;
        }

        [HttpGet("stream")]
        public async Task Stream(CancellationToken cancellationToken = default)
        {
            var encryptedUserId = HttpContext.User.Claims.FirstOrDefault(x => x.Type == UserClaimsName.UserId)?.Value;

            if(string.IsNullOrWhiteSpace(encryptedUserId))
            {
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
            var userId = _stringCipher.Decrypt(encryptedUserId);
            if (string.IsNullOrWhiteSpace(userId))
            {
                Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            var topic = SseTopicNames.NotificationUser(userId);

            HttpContext.Features.Get<IHttpResponseBodyFeature>()?.DisableBuffering();

            Response.StatusCode = StatusCodes.Status200OK;
            Response.ContentType = "text/event-stream";
            Response.Headers.CacheControl = "no-cache, no-transform";
            Response.Headers.Connection = "keep-alive";
            Response.Headers.Append("X-Accel-Buffering", "no");

            var subscription = _sseHub.Subscribe(topic);

            try
            {
                await Response.WriteAsync("retry: 5000\n: connected\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);

                using var heartbeatTimer = new PeriodicTimer(TimeSpan.FromSeconds(20));

                var messageTask = ReadMessageAsync(subscription.reader, cancellationToken);
                var heartbeatTask = SendHearbeatsAsync(heartbeatTimer, cancellationToken);


                await Task.WhenAny(messageTask, heartbeatTask);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Notification SSE disconnected.");
            }
            catch(IOException ex)
            {
                _logger.LogInformation(ex, "Notification SSE conection closed.");
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Notification SSE failed .");
            }
            finally
            {
                _sseHub.Unsubscribe(topic, subscription.connectionId);
            }

            async Task ReadMessageAsync(ChannelReader<SseCommandEnvelope> reader, CancellationToken token)
            {
                await foreach(var envelope in reader.ReadAllAsync(token))
                {
                    await SseResponseWriter.WriteEventAsync(Response, envelope, token);
                }
            }

            async Task SendHearbeatsAsync(PeriodicTimer timer, CancellationToken token)
            {
                while(await timer.WaitForNextTickAsync(token))
                {
                    await Response.WriteAsync($": heartbeat {DateTime.UtcNow:O}\n\n", token);
                    await Response.Body.FlushAsync(token);
                }
            }
        }
    }

}
