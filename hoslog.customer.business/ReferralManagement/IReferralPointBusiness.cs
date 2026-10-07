using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace hoslog.customer.business.ReferralManagement
{
    public interface IReferralPointBusiness
    {
        Task<(HttpStatusCode, object)> GetReferralPointInfo();
        Task<(HttpStatusCode, object)> GetTotalReferralPoint();
        Task<(HttpStatusCode, object)> GetReferralInfoStatus();

    }
}
