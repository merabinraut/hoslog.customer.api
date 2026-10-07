using Dapper;
using hoslog.customer.shared.Advertisement;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.domain.Advertisement
{
    public class AdvertisementRepository : IAdvertisementRepository
    {
        private readonly RepositoryDao _dao;

        public AdvertisementRepository(IConfiguration configuration)
        {
            _dao = new RepositoryDao(configuration);
        }

        public async Task<List<AdvertisementCommon>> GetAdvertisementList()
        {
            string proc = "apiproc_get_advertisement_list";
            var parameters = new DynamicParameters();
            var dbResp = await _dao.ExecuteAsyncList<AdvertisementCommon>(proc, parameters);
            return dbResp;
        }
    }
}
