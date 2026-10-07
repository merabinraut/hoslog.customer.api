using hoslog.customer.shared.ApiLog;
using hoslog.customer.shared.ExceptionLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hoslog.customer.domain.ExceptionLog
{
    public interface IExceptionLogRepository
    {
        Task<ExceptionLogDbResponse> InsertExpLogAsync(ExceptionLogCommon log);
    }
}
