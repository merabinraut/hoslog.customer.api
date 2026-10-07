using hoslog.customer.domain.ApiLog;
using hoslog.customer.domain.ExceptionLog;
using hoslog.customer.shared.ExceptionLog;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hoslog.customer.business.ExceptionLog
{
    public class ExceptionLogBusiness : IExceptionLogBusiness
    {
        private readonly IExceptionLogRepository _logRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ExceptionLogBusiness(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _logRepository = new ExceptionLogRepository(configuration);
            _httpContextAccessor = httpContextAccessor;
        }

        public void WriteExceptionLog(ExceptionLogCommon exceptionLogCommon, bool writeToFile = false)
        {
            var repoResp =  _logRepository.InsertExpLogAsync(exceptionLogCommon);
            //throw new NotImplementedException();
        }

        public void WriteExceptionLog(Exception ex, string ipAddress, string url)
        {

            try
            {
                var exceptionLogCommon = new ExceptionLogCommon
                {
                    ExceptionType = ex.GetType().FullName,
                    ExceptionMessage = ex.Message,
                    ExceptionScript = ex.StackTrace?.Trim() ?? string.Empty,
                    ExceptionSource = ex.Source,
                    //InnerExceptionMessage = ex.InnerException?.Message,
                    //InnerExceptionStackTrace = ex.InnerException?.StackTrace
                };

                if (_httpContextAccessor.HttpContext is not null)
                {
                    //logParam.UserName ??= _httpContextAccessor.HttpContext.GetUserName();
                    //logParam.UserAgent ??= _httpContextAccessor.HttpContext.GetUserAgent();
                    exceptionLogCommon.IpAddress ??= ipAddress;
                    //logParam.ControllerName ??= _httpContextAccessor.HttpContext.GetController();
                    //logParam.ActionName ??= _httpContextAccessor.HttpContext.GetAction();
                    //logParam.QueryString ??= _httpContextAccessor.HttpContext.GetQueryString();
                    //exceptionLogCommon.Headers ??= _httpContextAccessor.HttpContext.GetRequestHeaders();
                    exceptionLogCommon.RequestUrl ??= url;
                    exceptionLogCommon.HttpMethod ??= _httpContextAccessor.HttpContext.Request.Method;
                    
                    //if (logParam.RequestBody is null)
                    //    (_, logParam.RequestBody) = await _httpContextAccessor.HttpContext.GetRequestBodyAsStringAsync();
                }

                //logParam.MachineName ??= Environment.MachineName;
                //logParam.Environment ??= _hostEnv.EnvironmentName;
                //logParam.LogId ??= _httpContextAccessor.HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("D");

                _logRepository.InsertExpLogAsync(exceptionLogCommon);
            }
            catch (Exception) { }
            //throw new NotImplementedException();
        }
    }
}
