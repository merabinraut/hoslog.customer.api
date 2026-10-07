using Asp.Versioning;
using hoslog.customer.api.Filters;
using hoslog.customer.business.Conversion;
using hoslog.customer.common.Models.Conversion;
using hoslog.customer.domain.Conversion;
using hoslog.customer.shared;
using Microsoft.AspNetCore.Mvc;

namespace hoslog.customer.api.Controllers;

[Route("api/v{version:apiVersion}/conversion")]
[ApiVersion("1.0")]
public class ConversionController : BaseApiController
{
    private readonly IConversionBusiness _conversion;

    public ConversionController(IConversionBusiness conversion)
    {
        _conversion = conversion;
    }

    [HttpPost("session")]
    [LogRequest]
    public async Task<IActionResult> CreateSession(ConversionSessionRequestModel request, CancellationToken token)
    {
        var (statusCode, session) = await _conversion.CreateSessionAsync(request, token);
        return HandleResponseFromStatusCode(statusCode, session);
    }

    [HttpPost("events")]
    [LogRequest, LogResponse]
    public async Task<IActionResult> InsertEvents([FromBody] ConversionEventRequestModel request, CancellationToken token)
    {
        var session = await _conversion.GetRequiredAsync(HttpContext);
        var (statusCode, events) = await _conversion.InsertEventAsync(session, request, token);
        return HandleResponseFromStatusCode(statusCode, events);
    }

    [HttpGet("journeys/{journeyId:guid}")]
    public async Task<IActionResult> GetJourney(Guid journeyId, CancellationToken token)
    {
        var session = await _conversion.GetRequiredAsync(HttpContext);
        var (statusCode, journey) = await _conversion.GetJourneyAsync(journeyId, session.conversionSessionSno, token);
        return HandleResponseFromStatusCode(statusCode, journey);
    }

    [HttpPost("journeys")]
    [LogRequest, LogResponse]
    public async Task<IActionResult> StartJourney(ReservationJourneyRequestModel request, CancellationToken token)
    {
        var session = await _conversion.GetRequiredAsync(HttpContext);
        var (statusCode, journey) = await _conversion.StartJourneyAsync(session, request, token);
        return HandleResponseFromStatusCode(statusCode, journey);
    }
}