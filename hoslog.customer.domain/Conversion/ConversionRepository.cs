using Dapper;
using hoslog.customer.common.Models.Conversion;
using hoslog.customer.common.Utilities;
using hoslog.customer.shared;
using hoslog.customer.shared.Conversion;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.domain.Conversion;

public class ConversionRepository : IConversionRepository
{
    private readonly RepositoryDao _dao;

    public ConversionRepository(IConfiguration config)
    {
        _dao = new RepositoryDao(config, "AnalyticsConnection");
    }

    public async Task<ConversionSessionContext> CreateConversionSessionAsync(ConversionSessionRequestCommon request)
    {
        string sp = "analytics.apiproc_conversion_session_create";
        var param = new DynamicParameters();
        param.Add("@conversionSessionId", request.sessionId);
        param.Add("@currentCustomerId", request.customerId);
        param.Add("@currentAuthenticationState", request.authenticationState);
        param.Add("@initialReferrerUrl", request.initialReferrerUrl);
        param.Add("@userAgent", request.userAgent);
        param.Add("@deviceType", request.deviceType);
        param.Add("@landingPageUrl", request.landingPageUrl);
        param.Add("@timezoneOffsetMinutes", request.timeZoneOffsetMinutes);
        param.Add("@timeZoneId", request.timeZoneId);
        param.Add("@customerLocationJson", request.customerLocationJson);

        return await _dao.ExecuteAsync<ConversionSessionContext>(sp, param);
    }

    public async Task<ConversionSessionContext> GetSessionAsync(Guid? sessionId)
    {
        string sp = "analytics.apiproc_conversion_session_get";
        var param = new DynamicParameters();
        param.Add("@conversionSessionId", sessionId);
        return await _dao.ExecuteAsync<ConversionSessionContext>(sp, param);
    }

    public Task<ConversionJourneyResponseCommon> GetJourneyAsync(Guid? journeyId)
    {
        string sp = "analytics.apiproc_conversion_journey_get";
        var param = new DynamicParameters();
        param.Add("@reservationJourneyId", journeyId);
        return _dao.ExecuteAsync<ConversionJourneyResponseCommon>(sp, param);
    }

    public async Task TouchSessionAsync(long? sessionSno, long customerId)
    {
        string sp = "analytics.apiproc_conversion_session_touch";
        var param = new DynamicParameters();
        param.Add("@sessionSno", sessionSno);
        param.Add("@customerId", customerId);

        await _dao.ExecuteAsync<CommonDBResponse>(sp, param);
    }

    public async Task<ReservationJourneyResponseCommon> InsertJourneyAsync(ReservationJourneyRequestCommon request)
    {
        const string sp = "analytics.apiproc_conversion_journey_create";

        var param = new DynamicParameters();

        param.Add("@startedAuthenticationState", request.authenticationState);
        param.Add("@customerId", request.customerId);
        param.Add("@clubId", request.clubId);
        param.Add("@initialHostId", request.initialHostId);
        param.Add("@selectedHostId", request.selectedHostId);
        param.Add("@reservationJourneyId", request.reservationJourneyId);
        param.Add("@sourcePageType", request.sourcePageType);
        param.Add("@sourcePageUrl", request.sourcePageUrl);
        param.Add("@previousPageUrl", request.previousPageUrl);
        param.Add("@conversionSessionSno", request.conversionSessionSno);

        return await _dao.ExecuteAsync<ReservationJourneyResponseCommon>(sp, param);
    }
    public async Task<ReservationJourneyResponseCommon> UpdateJourneyAsync(ReservationJourneyRequestCommon request)
    {
        const string sp = "analytics.apiproc_conversion_journey_update";

        var param = new DynamicParameters();

        param.Add("@reservationJourneyId", request.reservationJourneyId);
        param.Add("@startedAuthenticationState", request.authenticationState);
        param.Add("@customerId", request.customerId);
        param.Add("@clubId", request.clubId);
        param.Add("@selectedHostId", request.selectedHostId);
        param.Add("@sourcePageType", request.sourcePageType);
        param.Add("@sourcePageUrl", request.sourcePageUrl);
        param.Add("@selectedPlanId", request.selectedPlanId);
        param.Add("@selectedDateTime", request.selectedDateTime);
        param.Add("@noOfPeople", request.noOfPeople);
        param.Add("@journeyStatus", request.journeyStatus);
        param.Add("@previousPageUrl", request.previousPageUrl);

        return await _dao.ExecuteAsync<ReservationJourneyResponseCommon>(sp, param);
    }

    public async Task<ConversionEventResponseCommon> InsertEventAsync(ConversionEventRequestCommon request)
    {
        string sp = "analytics.apiproc_conversion_event_create";
        var param = new DynamicParameters();
        param.Add("@eventId", request.eventId);
        param.Add("@conversionSessionSno", request.conversionSessionSno);
        param.Add("@reservationJourneyId", request.reservationJourneyId);
        param.Add("@eventName", request.eventName);
        param.Add("@stepCode", request.stepCode);
        param.Add("@eventStatus", request.eventStatus);
        param.Add("@eventSource", request.eventSource);
        param.Add("@actorCustomerId", request.actorCustomerId);
        param.Add("@authenticationState", request.authenticationState);
        param.Add("@relatedEntityType", request.relatedEntityType);
        param.Add("@relatedEntityId", request.relatedEntityId);
        param.Add("@pageUrl", request.pageUrl);
        param.Add("@previousPageUrl", request.previousPageUrl);
        param.Add("@timezoneOffsetMinutes", request.timezoneOffsetMinutes);
        param.Add("@propertiesJson", request.propertiesJson);
        param.Add("@requestTraceId", request.requestTraceId);
        param.Add("@clientUTCDate", request.clientUTCDate);
        param.Add("@pageType", request.pageType);

        return await _dao.ExecuteAsync<ConversionEventResponseCommon>(sp, param);
    }

}