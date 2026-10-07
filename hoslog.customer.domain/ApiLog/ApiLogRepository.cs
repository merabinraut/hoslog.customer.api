using Dapper;
using hoslog.customer.shared.ApiLog;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.domain.ApiLog
{
    public class ApiLogRepository : IApiLogRepository
    {
        private readonly RepositoryDao _dao;

        public ApiLogRepository(IConfiguration configuration)
        {
            _dao = new RepositoryDao(configuration);
        }
        public async Task<APILogDbResponse> LogInsertAsync(ApiLogCommon log)
        {
            string proc = "apiproc_insert_api_log";
            var parameters = new DynamicParameters();
            parameters.Add("@transactionId", log.TransactionId);
            parameters.Add("@partnerTxnId", log.PartnerTxnId);
            parameters.Add("@agentId", log.AgentId);
            parameters.Add("@userId", log.UserId);
            parameters.Add("@traceId", log.TraceId);
            parameters.Add("@functionName", log.FunctionName);
            parameters.Add("@requestUrl", log.RequestUrl);
            parameters.Add("@requestHeaders", log.RequestHeaders);
            parameters.Add("@requestPayload", log.RequestPayload);
            parameters.Add("@responseHttpStatus", log.ResponseHttpStatus);
            parameters.Add("@responsePayload", log.ResponsePayload);
            parameters.Add("@ipAddress", log.IPAddress);
            parameters.Add("@createdPlatform", log.CreatedPlatform);
            parameters.Add("@actionUser", log.ActionUser);

            var dbresp = await _dao.ExecuteAsync<APILogDbResponse>(proc, parameters);
            return dbresp;

        }

        public async Task<APILogDbResponse> LogUpdateAsync(ApiLogCommon log)
        {
            string proc = "apiproc_update_api_log";
            var parameters = new DynamicParameters();
            parameters.Add("@transactionId", log.TransactionId);
            parameters.Add("@partnerTxnId", log.PartnerTxnId);
            parameters.Add("@agentId", log.AgentId);
            parameters.Add("@userId", log.UserId);
            parameters.Add("@traceId", log.TraceId);
            parameters.Add("@functionName", log.FunctionName);
            // parameters.Add("@requestUrl", log.RequestUrl);
            // parameters.Add("@requestHeaders", log.RequestHeaders);
            // parameters.Add("@requestPayload", log.RequestPayload);
            parameters.Add("@responseHttpStatus", log.ResponseHttpStatus);
            parameters.Add("@responsePayload", log.ResponsePayload);
            parameters.Add("@ipAddress", log.IPAddress);
            parameters.Add("@createdPlatform", log.CreatedPlatform);
            parameters.Add("@actionUser", log.ActionUser);

            var dbresp = await _dao.ExecuteAsync<APILogDbResponse>(proc, parameters);
            return dbresp;
        }
    }
}
