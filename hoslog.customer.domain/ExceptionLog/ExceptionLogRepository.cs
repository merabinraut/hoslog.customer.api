using Dapper;
using hoslog.customer.shared.ApiLog;
using hoslog.customer.shared.ExceptionLog;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hoslog.customer.domain.ExceptionLog
{
    public class ExceptionLogRepository : IExceptionLogRepository
    {
        private readonly RepositoryDao _dao;

        public ExceptionLogRepository(IConfiguration configuration)
        {
            _dao = new RepositoryDao(configuration);
        }

        public async Task<ExceptionLogDbResponse> InsertExpLogAsync(ExceptionLogCommon log)
        {
            string proc = "apiproc_insert_exception_log";
            var parameters = new DynamicParameters();
            parameters.Add("@requestUrl", log.RequestUrl);
            parameters.Add("@httpMethod", log.HttpMethod);
            parameters.Add("@exceptionType", log.ExceptionType);
            parameters.Add("@exceptionSource", log.ExceptionSource);
            parameters.Add("@exceptionMessage", log.ExceptionMessage);
            parameters.Add("@exceptionScript", log.ExceptionScript);
            parameters.Add("@ipAddress", log.IpAddress);
            parameters.Add("@createdPlatform", log.CreatedPlatform);
            parameters.Add("@actionUser", log.CreatedBy);

            var dbresp = await _dao.ExecuteAsync<ExceptionLogDbResponse>(proc, parameters);
            return dbresp;
        }
    }
}
