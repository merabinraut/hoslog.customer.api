using System.ComponentModel.DataAnnotations;

namespace hoslog.customer.shared.Inquiries
{
    public class InquiriesRequestCommon : CommonDBRequest
    {
        /// <summary>
        /// Email address is required to post inquiries.
        /// </summary>
        ///  [Required]
        public string? name { get; set; }
        public string? emailAddress { get; set; }
        public string? mobileNumber { get; set; }
        public int? inquiryType { get; set; }
        public string? subject { get; set; }
        public string? description { get; set; }
        public string? attachment { get; set; }
        public string? companyName { get; set; }

    }
}
