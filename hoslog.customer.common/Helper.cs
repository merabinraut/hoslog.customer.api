using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace hoslog.customer.common
{
    public static class Helper
    {
        public static long ConvertToUnixTimeSeconds(DateTime dateTime) => ((DateTimeOffset)dateTime).ToUnixTimeSeconds();

        public static DateTime ConvertFromUnixTimeSeconds(long unixTimeSeconds) => DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds).DateTime;

        // public static long ConvertToUnixTimeMilliseconds(DateTime dateTime) => ((DateTimeOffset)dateTime).ToUnixTimeMilliseconds();

        // public static DateTime ConvertFromUnixTimeMilliseconds(long unixTimeMilliseconds) => DateTimeOffset.FromUnixTimeMilliseconds(unixTimeMilliseconds).DateTime;

        public static long ConvertToUnixTimeMilliseconds(DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Unspecified)
                dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            else if (dateTime.Kind == DateTimeKind.Local)
                dateTime = dateTime.ToUniversalTime();

            return ((DateTimeOffset)dateTime).ToUnixTimeMilliseconds();
        }

        public static DateTime ConvertFromUnixTimeMilliseconds(long unixTimeMilliseconds)
        {
            DateTimeOffset dto = DateTimeOffset.FromUnixTimeMilliseconds(unixTimeMilliseconds);
            return dto.UtcDateTime;
        }

        public static T MapObject<T>(this object item)
        {
            T sr = default(T);
            if (item != null)
            {
                var obj = JsonConvert.SerializeObject(item);
                sr = JsonConvert.DeserializeObject<T>(obj);
            }
            return sr;
        }

        public static List<T> MapObjects<T>(this object item)
        {
            List<T> sr = default(List<T>);
            if (item != null)
            {
                var obj = Newtonsoft.Json.JsonConvert.SerializeObject(item);
                sr = JsonConvert.DeserializeObject<List<T>>(obj);
            }
            return sr;
        }

        public static string GetIpAddress(this HttpContext context)
        {
            return context?.Connection?.RemoteIpAddress?.ToString();
        }

        public static string GenerateRandomPassword(int length)
        {
            const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
            const string numberChars = "0123456789";
            const string symbolChars = "!@#$%^&*";
            const string allChars = upperChars + lowerChars + numberChars + symbolChars;

            Random random = new Random();

            // Ensure at least one character of each type
            char upper = upperChars[random.Next(upperChars.Length)];
            char lower = lowerChars[random.Next(lowerChars.Length)];
            char number = numberChars[random.Next(numberChars.Length)];
            char symbol = symbolChars[random.Next(symbolChars.Length)];

            // Generate remaining random characters
            string remainingChars = new string(Enumerable.Repeat(allChars, length - 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            // Combine all characters and shuffle
            string password = upper.ToString() + lower.ToString() + number.ToString() + symbol.ToString() + remainingChars;
            return new string(password.ToCharArray().OrderBy(x => random.Next()).ToArray());
        }

        public static string GetSubdomain(string url, string[] baseDomains)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            try
            {
                var uri = new Uri(url.StartsWith("http") ? url : $"http://{url}");
                var host = uri.Host.ToLower();

                foreach (var baseDomain in baseDomains)
                {
                    if (host.EndsWith($".{baseDomain}"))
                    {
                        var subdomain = host.Replace($".{baseDomain}", "");
                        return subdomain.Split('.')[0]; // Get first part if nested
                    }
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
