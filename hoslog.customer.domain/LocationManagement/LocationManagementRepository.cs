using Dapper;
using hoslog.customer.shared.LocationManagement;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.domain.LocationManagement
{
    public class LocationManagementRepository : ILocationManagementRepository
    {
        private readonly RepositoryDao _dao;

        public LocationManagementRepository(IConfiguration configuration)
        {
            _dao = new RepositoryDao(configuration);
        }
        public async Task<List<LocationCommon>> GetLocationList()
        {
            string proc = "apiproc_cp_location_detail";
            var parameters = new DynamicParameters();
            parameters.Add("@flag", "gll");

            var dbResp = await _dao.ExecuteAsyncList<LocationCommon>(proc, parameters);
            return dbResp;
        }
    }
}
