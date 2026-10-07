using hoslog.customer.common.Models.ClubManagement;
using hoslog.customer.shared;
using System.Net;

namespace hoslog.customer.business.ClubManagement
{
    public interface IClubManagementBusiness
    {
        Task<(HttpStatusCode, object)> GetClubBasicInfoDetail(string clubId = "", string clubCode = "");
        Task<(HttpStatusCode, object)> GetClubDetailList(ClubDetailModel objClubDetailModel);
        Task<(HttpStatusCode, object)> GetClubReviewAndRatings(AmazonSettingsCommon mappedAwsSetting, string clubId = "", string clubCode = "", int pageNo = 1);
        Task<(HttpStatusCode, object)> GetClubWeeklySchedule(string agentId = "", string clubCode = "", int duration = 1);
        #region shops you can vist now
        Task<(HttpStatusCode, CommonApiResponse)> ShopsYouCanVisitNow(string locationId, int pageNo = 1);
        Task<(HttpStatusCode, CommonApiResponse)> ShopsYouCanVisitNowTest(string locationId, int pageNo = 1);
        #endregion
    }
}
