using hoslog.customer.shared.ExceptionLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hoslog.customer.business.ExceptionLog
{
    public interface IExceptionLogBusiness
    {
        //void LogException(Exception exception);
        void WriteExceptionLog(Exception ex, string ipAddress, string url);
        void WriteExceptionLog(ExceptionLogCommon exceptionLogCommon, bool writeToFile = false);

        
    }
}
