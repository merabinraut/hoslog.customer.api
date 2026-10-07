using System.Text.Json;
using Hoslog.Customer.Common.RealTime.Models;
using Microsoft.AspNetCore.Http;

namespace hoslog.customer.common.RealTime;

public static class SseResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static async Task WriteConnectedAsync(
        HttpResponse response,
        CancellationToken cancellationToken
    )
    {
        await response.WriteAsync(": connected\n\n", cancellationToken);
        await response.Body.FlushAsync(cancellationToken);
    }

    public static async Task WriteEventAsync(
        HttpResponse response,
        SseCommandEnvelope envelope,
        CancellationToken cancellationToken
    )
    {
        var json = JsonSerializer.Serialize(envelope, JsonOptions);

        await response.WriteAsync($"id: {envelope.id}\n", cancellationToken);
        await response.WriteAsync($"event: {envelope.eventName}\n", cancellationToken);
        await response.WriteAsync($"data: {json}\n\n", cancellationToken);

        await response.Body.FlushAsync(cancellationToken);
    }
}