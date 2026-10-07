using hoslog.customer.domain.ApiLog;
using hoslog.customer.shared.ApiLog;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.business.ApiLog
{
    public class ApiLogBusiness : IApiLogBusiness
    {
        private readonly IApiLogRepository _logRepository;

        public ApiLogBusiness(IConfiguration configuration)
        {
            _logRepository = new ApiLogRepository(configuration);
        }
        public async Task<string> InsertLog(ApiLogCommon requestLog)
        {
            var repoResp = await _logRepository.LogInsertAsync(requestLog);

            if (repoResp != null && repoResp.code == "0")
                return repoResp.LogId;
            return string.Empty;
        }

        public void UpdateLog(ApiLogCommon requestLog)
        {
            _logRepository.LogUpdateAsync(requestLog);
        }
    }
}
