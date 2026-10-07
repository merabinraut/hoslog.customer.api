using hoslog.customer.common.Models.EnqueryModel;
using hoslog.customer.shared;
using hoslog.customer.shared.Inquiries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace hoslog.customer.business.Inquiries
{
    public interface IInquiriesBusiness
    {
        Task<(HttpStatusCode, object)> PostInquiries(InquiriesRequestModel request,string path="");
    }
}
