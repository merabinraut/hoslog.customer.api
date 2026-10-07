using Dapper;
using hoslog.customer.shared.SSE;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.domain.SSE;

public class VisitNowRepository : IVisitNowRepository
{
    private readonly RepositoryDao _dao;
    public VisitNowRepository(IConfiguration configuration)
    {
        _dao = new RepositoryDao(configuration); 
    }

    public Task<List<VisitNowStatusChangedRow>> RecalculateStatusAsync(
        int locationId,
        string timeZoneOffsetValue
    )
    {
        string proc = "dbo.apiproc_cp_visit_now_status_recalculate";

        var parameters = new DynamicParameters(new
        {
            locationId,
            timeZoneOffsetValue
        });

        return _dao.ExecuteAsyncList<VisitNowStatusChangedRow>(proc, parameters);
    }
}