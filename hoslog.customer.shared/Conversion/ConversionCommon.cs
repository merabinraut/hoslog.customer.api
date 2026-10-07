namespace hoslog.customer.shared.Conversion;

public class ConversionCommon
{

}

public class ConversionSessionRequestCommon
{
    public Guid sessionId { get; set; }
    public long? customerId { get; set; }
    public string? authenticationState { get; set; }
    public string? landingPageUrl { get; set; }
    public string? initialReferrerUrl { get; set; }
    public string? deviceType { get; set; }
    public string? userAgent { get; set; }
    public string? timeZoneOffsetMinutes { get; set; }
    public string? timeZoneId { get; set; }
    public string? customerLocationJson { get; set; }
}
public class ReservationJourneyResponseCommon : CommonDBResponse
{
    public string? reservationJourneySno { get; set; }
    public string? conversionSessionSno { get; set; }
    public Guid? reservationJourneyId { get; set; }
}

public class ConversionEventResponseCommon : CommonDBResponse
{
    public Guid? eventId { get; set; }
    public string? conversionSessionSno { get; set; }
    public string? reservationJourneySno { get; set; }
    public string? eventName { get; set; }
    public string? eventStatus { get; set; }
    public string? eventSource { get; set; }
    public DateTime clientUTCDate { get; set; }
}

public class ConversionEventRequestCommon
{
    public Guid eventId { get; set; }
    public Guid? reservationJourneyId { get; set; }
    public string? conversionSessionSno { get; set; }
    public string? reservationJourneySno { get; set; }
    public string? eventName { get; set; }
    public string? stepCode { get; set; }
    public string? eventStatus { get; set; }
    public string? eventSource { get; set; }
    public string? actorCustomerId { get; set; }
    public string? authenticationState { get; set; }
    public string? relatedEntityType { get; set; }
    public string? relatedEntityId { get; set; }
    public string? pageUrl { get; set; }
    public string? previousPageUrl { get; set; }
    public string? timezoneOffsetMinutes { get; set; }
    public string? errorCode { get; set; }
    public string? errorCategory { get; set; }
    public string? errorMessageSafe { get; set; }
    public string? propertiesJson { get; set; }
    public string requestTraceId { get; set; }
    public DateTime? clientUTCDate { get; set; }
    public string? pageType { get; set; }
}

public class ConversionJourneyResponseCommon
{
    public long? reservationJourneySno { get; set; }
    public Guid reservationJourneyId { get; set; }
    public long? customerId { get; set; }
    public long clubId { get; set; }
    public string? initialHostId { get; set; }
    public string? selectedHostId { get; set; }
    public long? selectedPlanId { get; set; }
    public long? reservationId { get; set; }
    public string journeyStatus { get; set; }
    public string? lastStepCode { get; set; }
}

public class ReservationJourneyRequestCommon
{
    public Guid? reservationJourneyId { get; set; }
    public string? conversionSessionSno { get; set; }
    public string? authenticationState { get; set; }
    public long? customerId { get; set; }
    public string? clubId { get; set; }
    public string? initialHostId { get; set; }
    public string? selectedHostId { get; set; }
    public string? selectedPlanId { get; set; }
    // public string? reservationId { get; set; }
    public long? selectedDateTime { get; set; }
    public int? noOfPeople { get; set; }
    public string? journeyStatus { get; set; }
    public string? lastStepCode { get; set; }
    public string sourcePageType { get; set; }
    public string? sourcePageUrl { get; set; }
    public string? previousPageUrl { get; set; }
}