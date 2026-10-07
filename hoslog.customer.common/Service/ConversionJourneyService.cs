    using hoslog.customer.shared.Conversion;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    namespace hoslog.customer.common.Service;

    public static class ConversionJourneyService
    {
        public static bool EnsureJourneyCanContinue(HttpRequest request,ConversionJourneyResponseCommon journey, long? customerId,ILogger logger)
        {
            if (request == null)
            {
                logger.LogInformation("HttpRequest is null.");
                return false;
            }

            if (journey == null)
            {
                logger.LogInformation("Reservation journey was not found.");
                return false;
            }
            
            // if (journey.customerId.HasValue && customerId.HasValue && journey.customerId != customerId)
            // {
            //     logger.LogInformation($"Journey cannot continue because its customerId is '{journey.customerId}' and the customerId in the request is '{customerId}'.");
            //     return false;
            // }
            
            if (!Guid.TryParse(request.Headers["X-Reservation-Journey-Id"], out _))
            {
                logger.LogInformation("X-Reservation-Journey-Id is required and must be a UUID.");
                return false;
            }
            
            if (short.TryParse(request.Headers["X-Timezone-Offset-Minutes"], out var parsedOffset))
            {
                if (parsedOffset is < -840 or > 840)
                {
                    logger.LogInformation("X-Timezone-Offset-Minutes is outside the valid range.");
                    return false;
                }
            }

            return true;
        }
        
    }
