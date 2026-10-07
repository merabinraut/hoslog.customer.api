using Dapper;
using hoslog.customer.shared.PointManagement;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.domain.PointManagement
{
    public class PointManagementRepository : IPointManagementRepository
    {
        private readonly RepositoryDao _dao;
        public PointManagementRepository(IConfiguration configuration)
        {
            _dao = new RepositoryDao(configuration);
        }

        public async Task<List<PointManagementResponseCommon>> GetPoint(PointManagementRequestCommon request)
        {
            string proc = "apiproc_cp_get_customer_point_report";
            var parameters = new DynamicParameters(
                new
                {
                    request.agentId,
                    request.transactionMode,
                    request.row,
                    request.fetch
                });
            return await _dao.ExecuteAsyncList<PointManagementResponseCommon>(proc, parameters);
        }
    }
}