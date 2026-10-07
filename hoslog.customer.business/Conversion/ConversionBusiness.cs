using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Amazon.CognitoIdentityProvider;
using hoslog.customer.api.Library;
using hoslog.customer.common;
using hoslog.customer.common.Models.Auth;
using hoslog.customer.common.Models.Conversion;
using hoslog.customer.common.Models.IPService;
using hoslog.customer.common.Service;
using hoslog.customer.common.Service.IP;
using hoslog.customer.common.Utilities;
using hoslog.customer.domain.Conversion;
using hoslog.customer.shared;
using hoslog.customer.shared.Conversion;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace hoslog.customer.business.Conversion;

public class ConversionBusiness : IConversionBusiness
{
    private readonly IConversionRepository _repos;
    private readonly StringCipher _stringCipher;
    private readonly IHttpContextAccessor _context;
    private readonly IConfiguration _config;
    private readonly ValidationHelper _val;
    private readonly ApiResponseMessageService _messageService;
    private readonly ILogger<ConversionBusiness> _logger;

    private static readonly HashSet<string> AllowedClientEvents = new(StringComparer.OrdinalIgnoreCase)
    {
        "call_reservation_clicked",
        "online_reservation_clicked",
        "reservation_journey_started",
        "reservation_datetime_selected",
        "reservation_plan_selected",
        "reservation_host_selected",
        "reservation_preference_selected",
        "reservation_payment_method_selected",
        "reservation_confirmation_viewed",
        "reservation_cancelled",
        "reservation_step_failed",
        "reservation_journey_abandoned",
        "otp_expired",
    };

    private readonly IClientIpAddressAccessor
        _clientIpAddressAccessor;

    private readonly IIpGeolocationService
        _ipGeolocationService;

    private readonly DefaultCustomerLocationOptions _defaultLocation;

    public ConversionBusiness(ILogger<ConversionBusiness> logger, ApiResponseMessageService messageService, ValidationHelper val, IConfiguration config,
        IHttpContextAccessor context, StringCipher stringCipher,
        IConversionRepository repos, IClientIpAddressAccessor clientIpAddressAccessor,
        IIpGeolocationService ipGeolocationService, IOptions<DefaultCustomerLocationOptions> defaultLocation)
    {
        _logger = logger;
        _messageService = messageService;
        _val = val;
        _repos = repos;
        _stringCipher = stringCipher;
        _context = context;
        _config = config;

        _clientIpAddressAccessor =
           clientIpAddressAccessor;

        _ipGeolocationService =
            ipGeolocationService;

        _defaultLocation = defaultLocation.Value;
    }

    public async Task<(HttpStatusCode, object)> CreateSessionAsync(
        ConversionSessionRequestModel request, CancellationToken token)
    {
        var response = new CommonApiResponse()
        {
            errors = []
        };

        var validationResult = await _val.ValidateSignatureAsync(request);
        if (validationResult.Item1 != HttpStatusCode.OK)
            return validationResult;

        var httpContext = _context.HttpContext;
        var sessionCookie = _config["SessionCookie"] ?? string.Empty;
        var sessionIdleMinutes = _config.GetValue<long>("ConversionOptions:sessionIdleMinutes");

        var clientIp =
            _clientIpAddressAccessor.GetClientIpAddress(httpContext);

        var location = new CustomerIpLocation
        {
            ipAddress = clientIp?.ToString(),
            locationSource = "default",
            isApproximate = true,

            countryCode = _defaultLocation.countryCode,
            countryName = _defaultLocation.countryName,
            regionCode = _defaultLocation.regionCode,
            regionName = _defaultLocation.regionName,
            cityName = _defaultLocation.cityName,
            postalCode = _defaultLocation.postalCode,
            latitude = _defaultLocation.latitude,
            longitude = _defaultLocation.longitude,
            timeZone = _defaultLocation.timeZone,

            accuracyRadiusKilometers = null
        };

        if (clientIp is not null &&
            !IpAddressUtility.IsPrivateOrLocal(clientIp))
        {
            var maxMindLocation =
                _ipGeolocationService.FindLocation(clientIp);

            if (maxMindLocation is not null)
            {
                location =
                    maxMindLocation.MapObject<CustomerIpLocation>();

                location.ipAddress = clientIp.ToString();
                location.locationSource = "maxmind";
                location.isApproximate = true;
            }

        }

        long customerId = 0;
        var userAgent = httpContext?.Request.Headers.UserAgent.ToString();

        var userIdClaim = httpContext?.User.Claims
            .FirstOrDefault(x => x.Type == UserClaimsName.UserId)?.Value;

        var isAuthenticated = !string.IsNullOrEmpty(userIdClaim)
                              && long.TryParse(_stringCipher.Decrypt(userIdClaim), out customerId);

        var existingSessionId = GetExistingSessionId(httpContext, sessionCookie);

        if (existingSessionId.HasValue)
        {
            var existing = await _repos.GetSessionAsync(existingSessionId);

            if (existing != null)
            {
                var expired = existing.lastActivityUTCDate.AddMinutes(sessionIdleMinutes) < DateTime.UtcNow;

                if (!expired)
                {
                    WriteSessionCookie(httpContext, existing, sessionCookie);

                    response.code = ResponseCodes.Code200_Success;
                    response.message = ResponseMessages.Msg200_Success;
                    response.data = new ConversionSessionContext()
                    {
                        conversionSessionId = existing.conversionSessionId,
                        conversionSessionSno = _stringCipher.Encrypt(existing.conversionSessionSno.ToString()),
                        currentCustomerId = isAuthenticated ? _stringCipher.Encrypt(customerId.ToString()) : _stringCipher.Encrypt(existing.currentCustomerId),
                        authenticationState = existing.authenticationState,
                        lastActivityUTCDate = existing.lastActivityUTCDate,
                        startedUTCDate = existing.startedUTCDate,
                        closedUTCDate = existing.closedUTCDate,
                    };
                    return (HttpStatusCode.OK, response);
                }
            }
        }

        var sessionRequest = new ConversionSessionRequestCommon()
        {
            sessionId = Guid.NewGuid(),
            customerId = customerId,
            initialReferrerUrl = request.initialReferrerUrl,
            authenticationState = isAuthenticated ? "authenticated" : "guest",
            landingPageUrl = request.landingPageUrl,
            deviceType = ResolveDeviceType(userAgent),
            userAgent = userAgent,
            timeZoneOffsetMinutes = httpContext?.Items["TimeZoneOffsetMinutes"]?.ToString() ?? string.Empty,
            timeZoneId = httpContext?.Items["TimeZoneId"]?.ToString() ?? string.Empty,
            customerLocationJson = JsonSerializer.Serialize(location)
        };

        var session = await CreateSessionAsync(httpContext, sessionCookie, sessionRequest);

        response.code = ResponseCodes.Code200_Success;
        response.message = ResponseMessages.Msg200_Success;
        response.data = new ConversionSessionContext()
        {
            conversionSessionId = session.conversionSessionId,
            conversionSessionSno = _stringCipher.Encrypt(session.conversionSessionSno),
            currentCustomerId = _stringCipher.Encrypt(customerId.ToString()),
            authenticationState = sessionRequest.authenticationState,
            lastActivityUTCDate = DateTime.UtcNow,
            startedUTCDate = session.startedUTCDate,
            closedUTCDate = session.closedUTCDate
        };
        return (HttpStatusCode.OK, response);
    }

    private async Task<ConversionSessionContext> CreateSessionAsync(HttpContext httpContext, string sessionCookie,
        ConversionSessionRequestCommon sessionRequest)
    {
        var session = await _repos.CreateConversionSessionAsync(sessionRequest);
        WriteSessionCookie(httpContext, session, sessionCookie);
        return session;
    }

    public async Task<ConversionSessionContext> GetRequiredAsync(HttpContext? httpContext)
    {
        var sessionId = GetExistingSessionId(httpContext, _config["SessionCookie"] ?? string.Empty);

        if (!sessionId.HasValue)
        {
            _logger.LogInformation("Conversion session is missing. Initialize /api/v1/conversion/session first.");
            return new ConversionSessionContext();
        }

        var session = await _repos.GetSessionAsync(sessionId);

        if (session.lastActivityUTCDate.AddMinutes(_config.GetValue<long>("ConversionOptions:sessionIdleMinutes")) <
            DateTime.UtcNow)
        {
            _logger.LogInformation("Conversion session has expired. Initialize /api/v1/conversion/session first.");
            return new ConversionSessionContext();
        }

        long customerId = 0;
        var userIdClaim = httpContext?.User.Claims
            .FirstOrDefault(x => x.Type == UserClaimsName.UserId)?.Value;

        var isAuthenticated = !string.IsNullOrEmpty(userIdClaim)
                              && long.TryParse(_stringCipher.Decrypt(userIdClaim), out customerId);

        return new ConversionSessionContext()
        {
            conversionSessionId = session.conversionSessionId,
            conversionSessionSno = _stringCipher.Encrypt(session.conversionSessionSno),
            currentCustomerId = isAuthenticated ? _stringCipher.Encrypt(customerId.ToString()) : _stringCipher.Encrypt(session.currentCustomerId),
            authenticationState = isAuthenticated ? "authenticated" : session.authenticationState,
            lastActivityUTCDate = DateTime.UtcNow
        };
    }

    public async Task<(HttpStatusCode, object)> GetJourneyAsync(Guid? journeyId, string? sessionSno,
        CancellationToken token)
    {
        var response = new CommonApiResponse()
        {
            errors = []
        };
        var isValidGuid = Guid.TryParse(journeyId.ToString(), out var journeyGuid);

        if (!isValidGuid)
        {
            response.code = ResponseCodes.Code400_BadRequest;
            response.message = ResponseMessages.Msg400_BadRequest;
            response.errors = new List<Errors>()
            {
                new Errors()
                {
                    errorCode = ResponseCodes.CodeCUECM104_Invalid_journey_Id,
                    errorMessage =
                        await _messageService.GetMessageByCodeAsync(ResponseCodes.CodeCUECM104_Invalid_journey_Id)
                }
            };
            return (HttpStatusCode.BadRequest, response);
        }

        var journey = await _repos.GetJourneyAsync(journeyGuid);
        response.code = ResponseCodes.Code200_Success;
        response.message = ResponseMessages.Msg200_Success;
        response.data = journey;
        return (HttpStatusCode.OK, response);
    }

    public async Task<(HttpStatusCode, object)> StartJourneyAsync(ConversionSessionContext session,
        ReservationJourneyRequestModel request, CancellationToken token)
    {
        var response = new CommonApiResponse()
        {
            errors = []
        };


        var validationResult = await _val.ValidateSignatureAsync(request);
        if (validationResult.Item1 != HttpStatusCode.OK)
            return validationResult;

        var isValidGuid = Guid.TryParse(_context.HttpContext?.Request.Headers["X-Reservation-Journey-Id"].ToString(), out var reservationJourneyId);


        var referrer = _context.HttpContext?.Request.Headers.Referer.ToString() ?? string.Empty;

        var isPlanPage =
            Uri.TryCreate(referrer, UriKind.Absolute, out var uri) &&
            uri.AbsolutePath.EndsWith("/plan", StringComparison.OrdinalIgnoreCase);

        var isHostPage = Uri.TryCreate(referrer, UriKind.Absolute, out var uri2) && uri2.AbsolutePath.EndsWith("/host", StringComparison.OrdinalIgnoreCase);

        if (isValidGuid && (isPlanPage || isHostPage))
        {
            var journey = await _repos.GetJourneyAsync(reservationJourneyId);
            if (journey == null)
            {
                _logger.LogWarning("Journey not found for journeyId: {journeyId}", reservationJourneyId);
            }

            var journeyUpdate = await _repos.UpdateJourneyAsync(new ReservationJourneyRequestCommon()
            {
                reservationJourneyId = reservationJourneyId,
                conversionSessionSno = _stringCipher.Decrypt(session.conversionSessionSno.ToString()),
                authenticationState = _stringCipher.Decrypt(session.currentCustomerId) != "0" ? "authenticated" : "guest",
                customerId = long.Parse(_stringCipher.Decrypt(session.currentCustomerId)),
                clubId = _stringCipher.Decrypt(request.clubId),
                initialHostId = _stringCipher.Decrypt(request.initialHostId),
                selectedHostId = string.IsNullOrWhiteSpace(request.selectedHostId)
                    ? null
                    : string.Join(",",
                        request.selectedHostId
                            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .Select(id => _stringCipher.Decrypt(id))),
                selectedPlanId = _stringCipher.Decrypt(request.selectedPlanId),
                selectedDateTime = request.selectedDateTime,
                noOfPeople = request.noOfPeople,
                journeyStatus = request.journeyStatus,
                lastStepCode = request.lastStepCode,
                sourcePageType = request.sourcePageType,
                previousPageUrl = request.previousPageUrl
            });
            if (journeyUpdate.code != "0")
            {
                response.code = ResponseCodes.Code400_BadRequest;
                response.message = ResponseMessages.Msg400_BadRequest;
                return (HttpStatusCode.BadRequest, response);
            }
            var commonEvent = new
            {
                ConversionSessionSno = _stringCipher.Decrypt(session.conversionSessionSno),
                ReservationJourneySno = journeyUpdate.reservationJourneySno,
                ActorCustomerId = _stringCipher.Decrypt(session.currentCustomerId),
                AuthenticationState = _stringCipher.Decrypt(session.currentCustomerId) != "0" ? "authenticated" : "guest",
                PageUrl = URLHelper.Sanitize(request.sourcePageUrl),
                PreviousPageUrl = URLHelper.Sanitize(request.previousPageUrl),
                TraceId = Activity.Current?.TraceId.ToString()
            };

            var events = new List<(string? EventSourceName, string? RelatedEntityType, string? RelatedEntityId)>();
            var decryptedPlanId = _stringCipher.Decrypt(request.selectedPlanId);
            if (isPlanPage)
            {
                events.Add(("plan_selected", "plan", decryptedPlanId));
                events.Add(("date_time_selected", "time", request.selectedDateTime.ToString()));
                events.Add(("no_of_people_selected", "people", request.noOfPeople?.ToString()));

                if (!string.IsNullOrWhiteSpace(request.initialHostId))
                {
                    events.Add(("host_selected", "host",
                        _stringCipher.Decrypt(request.initialHostId)));
                }
            }
            else if (isHostPage)
            {
                if (!string.IsNullOrWhiteSpace(request.selectedHostId))
                {
                    var hostIds = request.selectedHostId
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(_stringCipher.Decrypt);

                    events.Add((
                        "host_selected",
                        "host",
                        string.Join(",", hostIds)
                    ));
                }
            }
            await Task.WhenAll(events.Select(e =>
                _repos.InsertEventAsync(new ConversionEventRequestCommon
                {
                    eventId = Guid.NewGuid(),
                    reservationJourneyId = reservationJourneyId,
                    conversionSessionSno = commonEvent.ConversionSessionSno,
                    reservationJourneySno = commonEvent.ReservationJourneySno,
                    eventName = e.Item1,
                    stepCode = e.Item1,
                    eventStatus = "success",
                    eventSource = "backend",
                    actorCustomerId = commonEvent.ActorCustomerId,
                    authenticationState = commonEvent.AuthenticationState,
                    relatedEntityType = e.Item2,
                    relatedEntityId = e.Item3,
                    pageUrl = commonEvent.PageUrl,
                    previousPageUrl = commonEvent.PreviousPageUrl,
                    requestTraceId = commonEvent.TraceId
                })));
            response.code = ResponseCodes.Code200_Success;
            response.message = ResponseMessages.Msg200_Success;
            return (HttpStatusCode.OK, response);
        }

        if (request.sourcePageType is not ("store_detail" or "host_detail"))
        {
            response.code = ResponseCodes.Code400_BadRequest;
            response.message = ResponseMessages.Msg400_BadRequest;
            response.errors = new List<Errors>()
            {
                new Errors()
                {
                    errorCode = ResponseCodes.CodeCUECM100_Invalid_source_page,
                    errorMessage =
                        await _messageService.GetMessageByCodeAsync(ResponseCodes.CodeCUECM100_Invalid_source_page)
                }
            };
            return (HttpStatusCode.BadRequest, response);
        }

        if (request.sourcePageType == "store_detail" && string.IsNullOrEmpty(request.clubId))
        {
            response.code = ResponseCodes.Code400_BadRequest;
            response.message = ResponseMessages.Msg400_BadRequest;
            response.errors = new List<Errors>()
            {
                new Errors()
                {
                    errorCode = ResponseCodes.CodeCUE106_Club_Id_is_required,
                    errorMessage =
                        await _messageService.GetMessageByCodeAsync(ResponseCodes.CodeCUE106_Club_Id_is_required)
                }
            };
            return (HttpStatusCode.BadRequest, response);
        }

        if (request.sourcePageType == "host_detail" && (string.IsNullOrEmpty(request.initialHostId) ||
                                                        string.IsNullOrEmpty(request.selectedHostId)))
        {
            response.code = ResponseCodes.Code400_BadRequest;
            response.message = ResponseMessages.Msg400_BadRequest;
            response.errors = new List<Errors>()
            {
                new Errors()
                {
                    errorCode = ResponseCodes.CodeCUECM101_HostId_required_for_host_detail_journey,
                    errorMessage =
                        await _messageService.GetMessageByCodeAsync(ResponseCodes
                            .CodeCUECM101_HostId_required_for_host_detail_journey)
                }
            };
            return (HttpStatusCode.BadRequest, response);
        }
        var journeyResponse = await _repos.InsertJourneyAsync(new ReservationJourneyRequestCommon()
        {
            reservationJourneyId = Guid.NewGuid(),
            conversionSessionSno = _stringCipher.Decrypt(session.conversionSessionSno),
            authenticationState = _stringCipher.Decrypt(session.currentCustomerId) != "0" ? "authenticated" : "guest",
            customerId = long.Parse(_stringCipher.Decrypt(session.currentCustomerId)),
            clubId = _stringCipher.Decrypt(request.clubId),
            initialHostId = _stringCipher.Decrypt(request.initialHostId),
            selectedHostId = _stringCipher.Decrypt(request.selectedHostId),
            selectedPlanId = _stringCipher.Decrypt(request.selectedPlanId),
            journeyStatus = request.journeyStatus,
            lastStepCode = request.lastStepCode,
            sourcePageType = request.sourcePageType,
            sourcePageUrl = URLHelper.Sanitize(request.sourcePageUrl),
            previousPageUrl = URLHelper.Sanitize(request.previousPageUrl)
        });


        if (journeyResponse.code != "0")
        {
            response.code = ResponseCodes.Code400_BadRequest;
            response.message = ResponseMessages.Msg400_BadRequest;
            return (HttpStatusCode.BadRequest, response);
        }

        await _repos.InsertEventAsync(new ConversionEventRequestCommon()
        {
            eventId = Guid.NewGuid(),
            conversionSessionSno = _stringCipher.Decrypt(session.conversionSessionSno),
            reservationJourneySno = journeyResponse.reservationJourneySno,
            eventName = "journey_started",
            stepCode = "journey_started",
            eventStatus = "success",
            eventSource = "backend",
            actorCustomerId = _stringCipher.Decrypt(session.currentCustomerId),
            authenticationState = _stringCipher.Decrypt(session.currentCustomerId) != "0" ? "authenticated" : "guest",
            relatedEntityType = !string.IsNullOrEmpty(request.initialHostId) ? request.sourcePageType : null,
            relatedEntityId = !string.IsNullOrEmpty(request.initialHostId)
                ? _stringCipher.Decrypt(request.initialHostId)
                : null,
            pageUrl = URLHelper.Sanitize(request.sourcePageUrl),
            previousPageUrl = URLHelper.Sanitize(request.previousPageUrl),
            requestTraceId = Activity.Current?.TraceId.ToString(),
        });

        response.code = ResponseCodes.Code200_Success;
        response.message = ResponseMessages.Msg200_Success;
        response.data = new ReservationJourneyResponseCommon()
        {
            reservationJourneySno = _stringCipher.Encrypt(journeyResponse.reservationJourneySno.ToString()),
            reservationJourneyId = journeyResponse.reservationJourneyId,
            conversionSessionSno = _stringCipher.Encrypt(journeyResponse.conversionSessionSno),
        };
        return (HttpStatusCode.OK, response);
    }

    public async Task<(HttpStatusCode, object)> InsertEventAsync(ConversionSessionContext session,
        ConversionEventRequestModel request,
        CancellationToken token)
    {

        var response = new CommonApiResponse()
        {
            errors = []
        };
        var validationResult = await _val.ValidateSignatureAsync(request);
        if (validationResult.Item1 != HttpStatusCode.OK)
            return validationResult;

        var customerId = session.currentCustomerId;
        if (!AllowedClientEvents.Contains(request.eventName))
        {
            response.code = ResponseCodes.Code400_BadRequest;
            response.message = ResponseMessages.Msg400_BadRequest;
            response.errors = new List<Errors>()
            {
                new Errors()
                {
                    errorCode = ResponseCodes.CodeCUECM102_Invalid_event_name,
                    errorMessage =
                        await _messageService.GetMessageByCodeAsync(ResponseCodes.CodeCUECM102_Invalid_event_name)
                }
            };
            return (HttpStatusCode.BadRequest, response);
        }

        var journeyGuid =
            Guid.TryParse(_context.HttpContext?.Request.Headers["X-Reservation-Journey-Id"], out var guid);

        var journey =
                await _repos.GetJourneyAsync(guid);

        if (journey?.reservationJourneySno > 0 && journeyGuid)
        {
            if (request.eventName != "reservation_cancelled" && journey.customerId.HasValue && !string.IsNullOrEmpty(customerId) && journey.customerId.ToString() != customerId)
            {
                response.code = ResponseCodes.Code403_Forbidden;
                response.message = ResponseMessages.Msg403_Forbidden;
                response.errors = new List<Errors>()
                {
                    new Errors()
                    {
                        errorCode = ResponseCodes.CodeCUECM103_The_reservation_belongs_to_another_customer,
                        errorMessage = await _messageService.GetMessageByCodeAsync(ResponseCodes
                            .CodeCUECM103_The_reservation_belongs_to_another_customer)
                    }
                };
                return (HttpStatusCode.Forbidden, response);
            }
        }
        else if (request.eventName is not ("online_reservation_clicked" or "call_reservation_clicked"))
        {
            response.message = ResponseMessages.Msg400_BadRequest;
            response.code = ResponseCodes.Code400_BadRequest;
            response.errors = new List<Errors>()
            {
                new Errors()
                {
                    errorCode = ResponseCodes.CodeCUECM102_Invalid_event_name,
                    errorMessage =
                        await _messageService.GetMessageByCodeAsync(ResponseCodes.CodeCUECM102_Invalid_event_name)
                }
            };
            return (HttpStatusCode.BadRequest, response);
        }

        var events = await _repos.InsertEventAsync(new ConversionEventRequestCommon()
        {
            eventId = Guid.NewGuid(),
            reservationJourneyId = guid,
            reservationJourneySno = _stringCipher.Decrypt(request.reservationJourneySno),
            conversionSessionSno = _stringCipher.Decrypt(session.conversionSessionSno),
            eventName = request.eventName,
            stepCode = request.stepCode,
            eventStatus = request.eventStatus,
            eventSource = "client",
            actorCustomerId = _stringCipher.Decrypt(customerId),
            authenticationState = session.authenticationState,
            relatedEntityType = request.relatedEntityType,
            relatedEntityId = _stringCipher.Decrypt(request.relatedEntityId),
            pageUrl = URLHelper.Sanitize(request.pageUrl),
            previousPageUrl = URLHelper.Sanitize(request.previousPageUrl),
            requestTraceId = Activity.Current?.TraceId.ToString(),
            propertiesJson = JsonSerializer.Serialize(request),
            pageType = request.pageType,
            timezoneOffsetMinutes = _context.HttpContext?.Items["TimeZoneOffsetMinutes"]?.ToString() ?? string.Empty,
        });
        if (events.code != "0")
        {
            response.code = ResponseCodes.Code400_BadRequest;
            response.message = ResponseMessages.Msg400_BadRequest;
            return (HttpStatusCode.BadRequest, response);
        }

        response.code = ResponseCodes.Code200_Success;
        response.message = ResponseMessages.Msg200_Success;
        events.conversionSessionSno = _stringCipher.Encrypt(events.conversionSessionSno);
        response.data = events;
        return (HttpStatusCode.OK, response);
    }


    private Guid? GetExistingSessionId(HttpContext? httpContext, string sessionCookie)
    {
        if (httpContext is null
            || !httpContext.Request.Cookies.TryGetValue(_config["SessionCookie"], out var protectedValue))
        {
            return null;
        }

        try
        {
            return Guid.TryParse(protectedValue, out var sessionId) ? sessionId : null;
        }
        catch
        {
            return null;
        }
    }

    private void WriteSessionCookie(HttpContext? httpContext, ConversionSessionContext session, string sessionCookie)
    {
        if (httpContext is null) return;

        httpContext.Response.Cookies.Append(
            sessionCookie,
            session.conversionSessionId.ToString(), new CookieOptions()
            {
                HttpOnly = true,
                Secure = _config.GetValue<bool>("ConversionOptions:cookieSecure"),
                SameSite = SameSiteMode.None,
                IsEssential = true,
                MaxAge = TimeSpan.FromMinutes(_config.GetValue<int>("ConversionOptions:sessionIdleMinutes")),
            });
    }

    private static string ResolveDeviceType(string? userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return "Unknown";
        var normalized = userAgent.ToLowerInvariant();
        if (normalized.Contains("ipad") || normalized.Contains("tablet")) return "tablet";
        if (normalized.Contains("mobile") || normalized.Contains("iphone") || normalized.Contains("android"))
            return "mobile";
        return "desktop";
    }
}