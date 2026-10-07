using hoslog.customer.common.Models.ClubManagement;
using hoslog.customer.shared.ClubManagement;

namespace hoslog.customer.domain.ClubManagement
{
    public interface IClubManagementRepository
    {
        Task<IList<ClubBasicInformationModel>> GetClubBasicInformation(string? userId, string clubAgentId = "", string clubCode = "", string timeZoneOffsetValue = "");
        Task<List<ClubDetailCommon>> GetClubDetailList(ClubDetailModel objClubDetailModel);
        Task<IList<ClubOwnPlanDetailModel>?> GetClubOwnPlanDetails(string? clubId);
        Task<IList<ClubPlanDetailModel>> GetClubPlanDetails(string? clubId);
        Task<List<ClubReviewsCommon>> GetClubReviewAndRatings(string clubAgentId = "", string clubCode = "", int pageNo = 1);
        Task<List<ClubScheduleCommon>> GetClubWeeklySchedule(string agentId = "", string clubCode = "", int duration = 1);
        #region shops you can vist now
        Task<List<ClubsYouCanVisitNowResponseCommon>> ShopsYouCanVisitNow(ClubsYouCanVisitNowRequestCommon request);
        #endregion
    }
}
