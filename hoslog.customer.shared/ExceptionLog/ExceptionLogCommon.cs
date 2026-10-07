using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hoslog.customer.shared.ExceptionLog
{
    public class ExceptionLogCommon
    {
        public string? ExceptionLogId { get; set; }
        public string? RequestUrl { get; set; }
        public string? HttpMethod { get; set; }
        public string? ExceptionType { get; set; }
        public string? ExceptionSource { get; set; }
        public string? ExceptionMessage { get; set; }
        public string? ExceptionScript { get; set; }
        public string? IpAddress { get; set; }
        public string? CreatedPlatform { get; set; }
        public string? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class ExceptionLogDbResponse : CommonDBResponse
    {
        public string LogId { get; set; } = string.Empty;
    }
}
