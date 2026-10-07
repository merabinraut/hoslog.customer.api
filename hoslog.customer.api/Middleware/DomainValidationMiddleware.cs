using System.Text.Json;
using System.Text.RegularExpressions;
using hoslog.customer.api.Models;
using hoslog.customer.api.Security.Options;
using hoslog.customer.shared;
using Microsoft.Extensions.Options;

namespace hoslog.customer.api.Middleware
{
    public sealed class DomainValidationMiddleware
    {
        private static readonly Regex _subDomainRegex = new(
           @"^[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?$",
            RegexOptions.IgnoreCase |
            RegexOptions.CultureInvariant |
            RegexOptions.Compiled
        );

        private readonly RequestDelegate _next;
        private readonly IOptionsMonitor<DomainValidationOptions> _optionsMonitor;
        private readonly IConfiguration _configuration;

        public DomainValidationMiddleware(
            RequestDelegate next,
            IOptionsMonitor<DomainValidationOptions> optionsMonitor,
            IConfiguration configuration
        )
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var options = _optionsMonitor.CurrentValue;

            //Alow preflight to proceed if configured
            if (options.SkipOptionsRequests && HttpMethods.IsOptions(context.Request.Method))
            {
                await _next(context);
                return;
            }

            var originHeader = context.Request.Headers["Origin"].FirstOrDefault();
            var refererHeader = context.Request.Headers["Referer"].FirstOrDefault();
            var headerValue = originHeader ?? refererHeader;
            headerValue = NormalizeHeader(headerValue);

            // if (string.IsNullOrEmpty(headerValue))
            // {
            //     if (options.RequireOriginHeader)
            //     {
            //         await WriteErrorAsync(context, StatusCodes.Status400BadRequest, "Missing Origin or Referer header.");
            //         return;
            //     }
            //     await _next(context);
            //     return;
            // }

            // var originUri = TryParseOrigin(headerValue);
            // if (originUri is null)
            // {
            //     await WriteErrorAsync(context, StatusCodes.Status400BadRequest, "Invalid Origin or Referer header.");
            //     return;
            // }

            // var normalized = NormalizeHostAndPort(originUri);

            // if (!IsAllowed(normalized))
            // {
            //     await WriteErrorAsync(context, StatusCodes.Status403Forbidden, "Domain not allowed.");
            //     return;
            // }

            SourceDomainInfo? headerInfo = null;
            if (!string.IsNullOrWhiteSpace(headerValue))
            {
                var allowedBaseHosts = options.AllowedBaseHosts
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(NormalizeConfiguredHost)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderByDescending(x => x.Length)
                    .ToArray();


                if (!TryParseAndValidateSource(
                    headerValue,
                    allowedBaseHosts,
                    out headerInfo
                ))
                {
                    await WriteErrorAsync(
                        context,
                        StatusCodes.Status403Forbidden,
                        "Domain is not allowed.");

                    return;
                }
            }

            context.Items["RequestSourceUrl"] = headerValue;
            context.Items[RequestSourceContextKeys.sourceUrl] =
                headerInfo?.uri?.AbsoluteUri;

            context.Items[RequestSourceContextKeys.sourceUri] =
                headerInfo?.uri;

            context.Items[RequestSourceContextKeys.baseHost] =
                headerInfo?.baseHost;

            context.Items[RequestSourceContextKeys.subdomain] =
                headerInfo?.subdomain;

            await _next(context);
        }

        private static string NormalizeConfiguredHost(string value) =>
            value
            .Trim()
            .TrimEnd('.')
            .ToLowerInvariant();

        private static string? NormalizeHeader(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            value = value.Trim();

            if (value.Equals(
                "null",
                StringComparison.OrdinalIgnoreCase
            ))
                return null;

            return value;
        }

        private bool TryParseAndValidateSource(
            string value,
            IReadOnlyCollection<string> allowedBaseHosts,
            out SourceDomainInfo? result)
        {
            result = null;

            if (!Uri.TryCreate(
                    value.Trim(),
                    UriKind.Absolute,
                    out var uri))
            {
                return false;
            }

            var options = _optionsMonitor.CurrentValue;
            bool enforceStrictOriginUrlValidation = options.EnforceStrictOriginUrlValidation;

            if (enforceStrictOriginUrlValidation)
            {
                if
                (
                    !uri.Scheme.Equals(
                    Uri.UriSchemeHttps,
                    StringComparison.OrdinalIgnoreCase)
                )
                {
                    return false;
                }

                if (uri.HostNameType != UriHostNameType.Dns)
                    return false;

                if (!string.IsNullOrWhiteSpace(uri.UserInfo))
                    return false;

                if (!uri.IsDefaultPort)
                    return false;
            }

            if (!TryResolveAllowedHost(
                    uri.IdnHost,
                    allowedBaseHosts,
                    out var baseHost,
                    out var subdomain))
            {
                return false;
            }

            result = new SourceDomainInfo
            {
                uri = uri,
                baseHost = baseHost,
                subdomain = subdomain
            };

            return true;
        }

        private static bool TryResolveAllowedHost(
            string host,
            IReadOnlyCollection<string> allowedBaseHosts,
            out string baseHost,
            out string subdomain)
        {
            baseHost = string.Empty;
            subdomain = string.Empty;

            var normalizedHost = host
                .Trim()
                .TrimEnd('.')
                .ToLowerInvariant();

            foreach (var allowedBaseHost in allowedBaseHosts)
            {
                if (normalizedHost.Equals(
                        allowedBaseHost,
                        StringComparison.OrdinalIgnoreCase))
                {
                    baseHost = allowedBaseHost;
                    return true;
                }

                var hostSuffix = "." + allowedBaseHost;
                if (!normalizedHost.EndsWith(
                        hostSuffix,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var candidateSubdomain =
                    normalizedHost[..^hostSuffix.Length];

                // Only one subdomain level is supported.
                if (candidateSubdomain.Contains('.'))
                    return false;

                if (!_subDomainRegex.IsMatch(candidateSubdomain))
                    return false;

                baseHost = allowedBaseHost;
                subdomain = candidateSubdomain;

                return true;
            }

            return false;
        }

        private sealed class SourceDomainInfo
        {
            public required Uri uri { get; init; }

            public string baseHost { get; init; } = string.Empty;

            public string subdomain { get; init; } = string.Empty;
        }

        private static async Task WriteErrorAsync(HttpContext context, int statusCode, string message)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var response = new CommonApiResponse
            {
                code = statusCode.ToString(),
                message = message,
                data = null,
                errors = new List<Errors>()
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }

        private static string NormalizeHostAndPort(Uri uri)
        {
            var host = uri.Host.ToLowerInvariant();
            if (!uri.IsDefaultPort)
                return $"{host}:{uri.Port}";
            return host;
        }

        private record AllowedOriginsCache(HashSet<string> exactHosts, string[] wildCardSuffixes)
        {
            public static AllowedOriginsCache Empty =>
                new(new HashSet<string>(StringComparer.OrdinalIgnoreCase), Array.Empty<string>());
        }

        private static string GetHostOnly(string normalizedOrigin)
        {
            if (string.IsNullOrWhiteSpace(normalizedOrigin))
                return normalizedOrigin;

            var trimmed = normalizedOrigin.Trim();
            var colonIndex = trimmed.LastIndexOf(':');

            // If there's no colon, return as-is
            if (colonIndex <= 0)
                return trimmed;

            // Extract everything before the last colon as the host
            return trimmed.Substring(0, colonIndex);
        }
    }
}