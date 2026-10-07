namespace hoslog.customer.api.Security.Options
{
    public sealed class DomainValidationOptions
    {
        /// <summary>
        /// Allowed origins. Ecample: "cus.hoslog.jp", "https://cus.hoslog.jp", "*.hoslog.jp"
        /// </summary>
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();

        ///<summary>
        /// If true, requests without Origin/Referer will be rejected.
        /// Set to false to allow server-to-server calls or tooling that doesn't send these headers.
        /// </summary>
        public bool RequireOriginHeader { get; set; } = true;

        ///<summary>
        /// If true, middleware will bypass validation for HTTP OPTIONS requests (preflight).
        /// </summary>
        public bool SkipOptionsRequests { get; set; } = true;
        public bool EnforceStrictOriginUrlValidation { get; set; } = true;
        public string[] AllowedBaseHosts { get; set; } = [];
    }
}