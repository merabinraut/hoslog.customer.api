using hoslog.customer.shared;
using hoslog.customer.shared.Inquiries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hoslog.customer.domain.Inquiries
{
    public interface IInquiriesRepository
    {
        Task<CommonDBResponse> PostInquiriesAsync(InquiriesRequestCommon request);
    }
}
