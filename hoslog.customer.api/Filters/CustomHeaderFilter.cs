using System.Globalization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace hoslog.customer.api.Filters
{
    public class CustomHeaderFilter : ActionFilterAttribute
    {
        private readonly ILogger<CustomHeaderFilter> _logger;
        private readonly List<string> _headerKeys;
        private readonly IConfiguration _configuration;

        public CustomHeaderFilter(ILogger<CustomHeaderFilter> logger, List<string> headerKeys, IConfiguration configuration)
        {
            _logger = logger;
            _headerKeys = headerKeys;
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;

            foreach (var headerKey in _headerKeys)
            {
                if (request.Headers.TryGetValue(headerKey, out var headerValue))
                {
                    _logger.LogInformation($"{headerKey}: {headerValue}");
                }
                else
                {
                    _logger.LogWarning($"{headerKey} is missing in the request.");
                }
            }

            var language = ResolveLanguage(request);
            context.HttpContext.Items["Language"] = language;
            context.HttpContext.Items["TimezoneOffset"] = request.Headers["x-timezone-offset"].FirstOrDefault() ?? _configuration["DefaultTimeZoneOffsetValue"];
            context.HttpContext.Items["TimeZoneId"] = request.Headers["x-timezone-id"].FirstOrDefault();
            context.HttpContext.Items["TimeZoneOffsetMinutes"] = request.Headers["x-timezone-offset-minutes"].FirstOrDefault();
            base.OnActionExecuting(context);
        }

        private static string ResolveLanguage(HttpRequest request)
        {
            var headerLang = request.Headers["Accept-Language"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(headerLang))
                return NormalizeLanguage(headerLang);

            return "ja"; // default japanese
        }

        private static string NormalizeLanguage(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "ja";

            var firstPart = raw.Split(',')[0];

            firstPart = firstPart.Split(';')[0].Trim();

            try
            {
                var culture = CultureInfo.CreateSpecificCulture(firstPart);
                return culture.TwoLetterISOLanguageName.ToLowerInvariant();
            }
            catch
            {
                var simple = firstPart.Split('-')[0];
                return string.IsNullOrWhiteSpace(simple)
                    ? "ja"
                    : simple.ToLowerInvariant();
            }
        }
    }
}
