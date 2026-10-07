using hoslog.customer.shared.ApiLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hoslog.customer.domain.ApiLog
{
    public interface IApiLogRepository
    {
        Task<APILogDbResponse> LogInsertAsync(ApiLogCommon log);
        Task<APILogDbResponse> LogUpdateAsync(ApiLogCommon log);
    }
}
