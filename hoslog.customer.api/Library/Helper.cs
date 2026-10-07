using FluentValidation.Results;
using hoslog.customer.shared;
using Newtonsoft.Json;
using System.Net;

namespace hoslog.customer.api.Library
{
    public static class Helper
    {
        public static Task<(HttpStatusCode statusCode, CommonApiResponse response)> GenerateModelValidationResponse(ValidationResult request)
        {
            var response = new CommonApiResponse
            {
                code = ResponseCodes.Code400_BadRequest,
                message = ResponseCodes.GetMessageForResponseCode(ResponseCodes.Code400_BadRequest),
                errors = request.Errors.Select(error => new Errors
                {
                    errorCode = error.ErrorCode,
                    errorMessage = error.ErrorMessage
                }).ToList()
            };
            return Task.FromResult((HttpStatusCode.BadRequest, response));
        }

        public static long ConvertToUnixTimeSeconds(DateTime dateTime) => ((DateTimeOffset)dateTime).ToUnixTimeSeconds();

        public static DateTime ConvertFromUnixTimeSeconds(long unixTimeSeconds) => DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds).DateTime;

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
    }
}
