using hoslog.customer.common.Utilities;

namespace hoslog.customer.api.Library
{
    public class URLHelper
    {
        private readonly StringCipher _stringCipher;
        public URLHelper(StringCipher stringCipher)
        {
            _stringCipher = stringCipher;
        }
        public string EncryptQueryParams(string url)
        {
            Uri uri = new Uri(url);
            string queryString = uri.Query.TrimStart('?');

            string[] queryParams = queryString.Split('&');

            string encryptedQueryString = "";
            foreach (string param in queryParams)
            {
                if (string.IsNullOrEmpty(param))
                    continue;
                string[] parts = param.Split('=');
                string paramName = parts[0];
                string paramValue = parts[1];

                string encryptedValue = _stringCipher.Encrypt(paramValue);
                encryptedQueryString += $"{paramName}={encryptedValue}&";
            }
            if (!string.IsNullOrEmpty(queryString) && !string.IsNullOrEmpty(encryptedQueryString))
            {
                string encryptedUrl = url.Replace(queryString, encryptedQueryString.TrimEnd('&'));
                return encryptedUrl;
            }
            return url;
        }

        public static string ToUrlSafeBase64(string base64String)
        {
            return base64String
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");
        }

        public static string FromUrlSafeBase64(string urlSafeBase64)
        {
            return urlSafeBase64
                .Replace('-', '+')
                .Replace('_', '/')
                .PadRight(urlSafeBase64.Length + (urlSafeBase64.Length % 4 == 0 ? 0 : 4 - urlSafeBase64.Length % 4), '=');
        }
        
        public static string Sanitize(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }
            var withoutFragment = url.Split('#',2)[0];
            var pathOnly = withoutFragment.Split('?',2)[0];
            
            if(Uri.TryCreate(pathOnly,UriKind.Absolute,out var absoluteUri))
                pathOnly = absoluteUri.AbsolutePath;

            if (!pathOnly.StartsWith("/"))
                return null;
            
            return pathOnly.Length <= 1000 ? pathOnly : pathOnly[..1000];
        }
    }
}
