using System.Net;
using hoslog.customer.common.Models.Conversion;
using hoslog.customer.shared;
using Microsoft.AspNetCore.Http;

namespace hoslog.customer.business.Conversion;

public interface IConversionBusiness
{
    Task<(HttpStatusCode,object)> CreateSessionAsync(ConversionSessionRequestModel request,
        CancellationToken token);

    Task<ConversionSessionContext> GetRequiredAsync(HttpContext? httpContext);
    Task<(HttpStatusCode,object)> InsertEventAsync(ConversionSessionContext session,ConversionEventRequestModel request,CancellationToken token);
    Task<(HttpStatusCode,object)> GetJourneyAsync(Guid? journeyId, string? sessionSno,CancellationToken token);
    Task<(HttpStatusCode,object)> StartJourneyAsync(ConversionSessionContext session,ReservationJourneyRequestModel request,CancellationToken token);
}