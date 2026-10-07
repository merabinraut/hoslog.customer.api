using hoslog.customer.common.Models.Conversion;
using hoslog.customer.shared;
using hoslog.customer.shared.Conversion;

namespace hoslog.customer.domain.Conversion;

public interface IConversionRepository
{
    Task<ConversionSessionContext> CreateConversionSessionAsync(ConversionSessionRequestCommon request);
    Task<ConversionSessionContext> GetSessionAsync(Guid? sessionId);
    Task<ConversionEventResponseCommon> InsertEventAsync(ConversionEventRequestCommon request);
    Task<ConversionJourneyResponseCommon> GetJourneyAsync(Guid? journeyId);
    Task<ReservationJourneyResponseCommon> InsertJourneyAsync(ReservationJourneyRequestCommon request);
    Task<ReservationJourneyResponseCommon> UpdateJourneyAsync(ReservationJourneyRequestCommon request);
}