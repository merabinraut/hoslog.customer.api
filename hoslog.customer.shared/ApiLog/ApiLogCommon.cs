using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace hoslog.customer.shared.ApiLog
{
    public class ApiLogCommon
    {
        public string? ReferenceId { get; set; }
        public string? TransactionId { get; set; }
        public string? PartnerTxnId { get; set; }
        public string? AgentId { get; set; }
        public string? UserId { get; set; }
        public string? TraceId { get; set; }
        public string? FunctionName { get; set; }
        public string? RequestUrl { get; set; }
        public string? RequestHeaders { get; set; }
        public string? RequestPayload { get; set; }
        public string? ResponseHttpStatus { get; set; }
        public string? ResponsePayload { get; set; }
        public string? ActionUser { get; set; }
        public string? IPAddress { get; set; }
        public string? CreatedPlatform { get; set; }
    }

    public class APILogDbResponse : CommonDBResponse
    {
        public string LogId { get; set; } = string.Empty;
    }
}
