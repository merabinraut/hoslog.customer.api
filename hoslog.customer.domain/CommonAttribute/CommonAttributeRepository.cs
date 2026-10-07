using Dapper;
using hoslog.customer.shared;
using hoslog.customer.shared.Common;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.domain.CommonAttribute
{
    public class CommonAttributeRepository : ICommonAttributeRepository
    {
        private readonly RepositoryDao _dao;

        public CommonAttributeRepository(IConfiguration configuration)
        {
            _dao = new RepositoryDao(configuration);
        }

        public async Task<List<AttributeCommon>> GetCommonAttributeList(string flag)
        {
            string proc = "apiproc_cp_dropdown";
            var parameters = new DynamicParameters();
            parameters.Add("@flag", flag);

            var dbResp = await _dao.ExecuteAsyncList<AttributeCommon>(proc, parameters);
            return dbResp;
        }

        public async Task<CommonDBResponse> UpdateReferralClickCount(ReferralClickCountUpdateRequestCommon request)
        {
            string proc = "dbo.apiproc_cp_affiliate_referral_count_manage";
            var parameters = new DynamicParameters(new
            {
                request.referId,
                request.referCode
            });

            return await _dao.ExecuteAsync<CommonDBResponse>(proc, parameters);
        }
    }
}
