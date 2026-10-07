using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hoslog.customer.shared.ApiMessageManagement
{
    public class ApiMessageCommon
    {
        public string? sno { get; set; }
        public string? code { get; set; }
        public string? message { get; set; }
        public string? messageEng { get; set; }
        public string? category { get; set; }
        public string? description { get; set; }
        public string? messageType { get; set; }
        public string? httpStatusCode { get; set; }
    }

    public class ApiMessageRequestCommon
    {
        public string? messageId { get; set; }
        public string? searchFilter { get; set; }
    }
}
