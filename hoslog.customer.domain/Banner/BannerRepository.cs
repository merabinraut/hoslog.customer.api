using Dapper;
using hoslog.customer.shared.Banner;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.domain.Banner
{
    public class BannerRepository : IBannerRepository
    {
        private readonly RepositoryDao _dao;
        public BannerRepository(IConfiguration configuration)
        {
            _dao = new RepositoryDao(configuration);
        }

        public async Task<List<BannerCommon>> GetBannerList()
        {
            string proc = "dbo.sp_customer_dashboard";
            var parameters = new DynamicParameters();
            parameters.Add("@flag", "bl");

            return await _dao.ExecuteAsyncList<BannerCommon>(proc, parameters);
        }
    }
}
