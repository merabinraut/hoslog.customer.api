using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace hoslog.customer.shared
{
    public class CommonApiResponse
    {
        private string _responseDetailMessage;
        private string _responseMessage;
        private List<Errors> _fieldErrors;
        private string _responseCode;
        //private string _responseStatus;
        [JsonProperty(Order = 0)]
        [JsonPropertyOrder(0)]
        [Newtonsoft.Json.JsonIgnore]
        public string? code
        {
            get => _responseCode ?? string.Empty;
            set => _responseCode = value;
        }
        [JsonProperty(Order = 1)]
        [JsonPropertyOrder(1)]
        [Newtonsoft.Json.JsonIgnore]
        public string? message
        {
            get => _responseMessage ?? ResponseCodes.GetMessageForResponseCode(code);
            set => _responseMessage = value;
        }
        [JsonProperty(Order = 2)]
        [JsonPropertyOrder(2)]
        [Newtonsoft.Json.JsonIgnore]
        public object data { get; set; }
        [JsonProperty(Order = 3)]
        [JsonPropertyOrder(3)]
        [Newtonsoft.Json.JsonIgnore]
        public List<Errors>? errors
        {
            get => _fieldErrors ?? new();
            set => _fieldErrors = value;
        }
    }

    public class Errors
    {
        [JsonProperty(Order = 0)]
        [JsonPropertyOrder(0)]
        public string? errorCode { get; set; }
        [JsonProperty(Order = 1)]
        [JsonPropertyOrder(1)]
        public string? errorMessage { get; set; }
    }

    public class ImageResponse
    {
        public string? alt { get; set; }
        public string? description { get; set; }
        public string? path { get; set; }
    }
    public class LocationResponse
    {
        public string? url { get; set; }
        public string? longitude { get; set; }
        public string? latitude { get; set; }
    }
    public class PaginationResponse
    {
        public int? pageNo { get; set; } = 0;
        public int? totalRecords { get; set; } = 0;
        public int? recordPerPage { get; set; } = 10;
    }

    public interface IPaginationResponse
    {
        int? pageNo { get; set; }
        int? totalRecords { get; set; }
        int? recordPerPage { get; set; }
    }
    public class PaginationRequest
    {
        public int? pageNo { get; set; }
    }
}
