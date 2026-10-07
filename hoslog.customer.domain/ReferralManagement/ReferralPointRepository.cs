using Dapper;
using hoslog.customer.common.Models.Referral;
using hoslog.customer.shared.ReferralManagement;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.domain.ReferralManagement
{
    public class ReferralPointRepository : IReferralPointRepository
    {
        private readonly RepositoryDao _dao;

        public ReferralPointRepository(IConfiguration configuration)
        {
            _dao = new RepositoryDao(configuration);
        }

        public async Task<List<ReferralPointInfoCommon>> GetReferralPointInfo()
        {
            string proc = "apiproc_customer_get_referral_point_info";
            var parameters = new DynamicParameters();

            var dbResp = await _dao.ExecuteAsyncList<ReferralPointInfoCommon>(proc, parameters);
            return dbResp;
        }
        public async Task<ReferralPointTotalCommon> GetTotalReferralPoint(string customerId)
        {
            string proc = "apiproc_customer_get_total_referral_points";

            var parameters = new DynamicParameters();
            parameters.Add("@customerId", customerId);

            var dbResp = await _dao.ExecuteAsync<ReferralPointTotalCommon>(proc,parameters);
            return dbResp;
        }
        public async Task<ReferralInfoStatusCommon> GetReferralInfoStatus(string customerId)
        {
            string proc = "apiproc_customer_get_referral_introduction_status";
            var parameters = new DynamicParameters();
            parameters.Add("@customerId", customerId);
            var dbResp = await _dao.ExecuteAsync<ReferralInfoStatusCommon>(proc,parameters);
            return dbResp;
        }
    }
}
