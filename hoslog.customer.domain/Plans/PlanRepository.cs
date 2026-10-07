using Azure;
using Dapper;
using hoslog.customer.shared.Plans;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.domain.Plans
{
    public class PlanRepository : IPlanRepository
    {
        private readonly RepositoryDao _dao;

        public PlanRepository(IConfiguration configuration)
        {
            _dao = new RepositoryDao(configuration);
        }
        public async Task<List<PlanCommon>> GetAdminPlanList()
        {
            string proc = "apiproc_cp_adminplan_detail";
            var parameters = new DynamicParameters();
            parameters.Add("@flag", "gapl");

            var dbResp = await _dao.ExecuteAsyncList<PlanCommon>(proc, parameters);
            return dbResp;
        }

        public async Task<List<ClubPlanResponseCommon>> GetClubPlanList(string clubId)
        {
            string proc = "apiproc_cp_get_club_own_plan";
            var parameters = new DynamicParameters();
            parameters.Add("@clubId", clubId);
            var dbResp = await _dao.ExecuteAsyncList<ClubPlanResponseCommon>(proc, parameters);
            return dbResp;
        }

        public async Task<List<ClubPlanListResponseCommon>> GetClubAllPlanList(ClubPlanListRequestCommon request)
        {
            string proc = "apiproc_cp_get_club_plan_list";
            var parameters = new DynamicParameters();
            parameters.Add("@clubId", request.clubId);
            parameters.Add("@clubCode", request.clubCode);
            parameters.Add("@pageNo", request.pageNo);
            return await _dao.ExecuteAsyncList<ClubPlanListResponseCommon>(proc, parameters);
        }
    }
}
