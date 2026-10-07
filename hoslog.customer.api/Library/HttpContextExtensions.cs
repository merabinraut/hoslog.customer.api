using Newtonsoft.Json;
using System.Text;

namespace hoslog.customer.api.Library
{
    public static class HttpContextExtensions
    {
        public static async Task<(bool isForm, string body)> GetRequestBodyAsStringAsync(this HttpContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            if (!context.Request.Body.CanRead)
                return (false, null);

            string requestBody;
            if (context.Request.HasFormContentType)
            {
                requestBody = context.GetRequestBodyFormDataAsString();
                //requestBody = context.GetRequestBodyFormDataAsJsonDictionaryString();
                return (true, requestBody);
            }

            var buffer = await context.GetRequestBodyAsByteArrayAsync();
            requestBody = Encoding.UTF8.GetString(buffer);

            return (false, requestBody);
        }

        public static string GetRequestBodyFormDataAsString(this HttpContext context)
        {
            context.Request.EnableBuffering();

            var form = context.Request.Form;
            var formData = new List<string>();
            foreach (var key in form.Keys)
            {
                if (form[key].Count > 1)
                {
                    // If there are multiple values for the same key, add them all to the list.
                    foreach (var value in form[key])
                    {
                        if (!string.IsNullOrEmpty(value) && !IsFile(value))
                            formData.Add($"{key}={value}");
                    }
                }
                else
                {
                    // If there is only one value for the key, add it to the list.
                    var value = form[key];
                    if (!string.IsNullOrEmpty(value) && !IsFile(value))
                        formData.Add($"{key}={value}");
                }
            }

            context.Request.Body.Position = 0;

            return string.Join("&", formData);
        }
        public static async Task<byte[]> GetRequestBodyAsByteArrayAsync(this HttpContext context)
        {
            context.Request.EnableBuffering();

            var buffer = new byte[Convert.ToInt32(context.Request.ContentLength)];
            _ = await context.Request.Body.ReadAsync(buffer.AsMemory(0, buffer.Length));

            context.Request.Body.Position = 0;

            return buffer;
        }
        public static string GetRequestUrl(this HttpContext context)
        {
            return $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
        }

        //public static string GetIpAddress(this HttpContext context)
        //{
        //    return context?.Connection?.RemoteIpAddress?.ToString();
        //}

        public static string GetController(this HttpContext context)
        {
            return context.Request.RouteValues["controller"]?.ToString();
        }

        public static string GetAction(this HttpContext context)
        {
            return context.Request.RouteValues["action"]?.ToString();
        }

        public static string GetQueryString(this HttpContext context)
        {
            return context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty;
        }

        public static string GetUserAgent(this HttpContext context)
        {
            context.Request.Headers.TryGetValue("User-Agent", out var userAgent);
            return userAgent;
        }

        private static bool IsFile(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            if (value.StartsWith("filename=", StringComparison.OrdinalIgnoreCase) || value.StartsWith("content-type=", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        public static string GetRequestHeadersAsDictionaryJson(this HttpContext context)
        {
            var headersDict = new Dictionary<string, string>();

            foreach (var header in context.Request.Headers)
                headersDict.Add(header.Key, header.Value);

            return JsonConvert.SerializeObject(headersDict);
        }
    }
}
