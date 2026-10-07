using hoslog.customer.shared.ApiLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hoslog.customer.business.ApiLog
{
    public interface IApiLogBusiness
    {
        Task<string> InsertLog(ApiLogCommon requestLog);

        void UpdateLog(ApiLogCommon requestLog);
    }
}
