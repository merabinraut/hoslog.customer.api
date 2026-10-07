using hoslog.customer.shared.Plans;

namespace hoslog.customer.domain.Plans
{
    public interface IPlanRepository
    {
        Task<List<PlanCommon>> GetAdminPlanList();
        Task<List<ClubPlanResponseCommon>> GetClubPlanList(string clubId);
        Task<List<ClubPlanListResponseCommon>> GetClubAllPlanList(ClubPlanListRequestCommon request);
    }
}
