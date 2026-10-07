using System.Net;
using hoslog.customer.shared;

namespace hoslog.customer.business.Multitenant
{
    public interface IClubInformationMultitenantBusiness
    {
        Task<(HttpStatusCode, CommonApiResponse)> GetClubBasicInfomration(string clubCode = "", string clubId = "");
        Task<(HttpStatusCode, CommonApiResponse)> GetClubReviewAndRatings(string clubId = "", string clubCode = "", int pageNo = 0);
        Task<(HttpStatusCode, CommonApiResponse)> GetClubWeeklySchedule(string agentId = "", string clubCode = "", int duration = 1);
        Task<(HttpStatusCode, CommonApiResponse)> GetScheduleListAsync(string clubId, string clubCode, long? date);
        Task<(HttpStatusCode, CommonApiResponse)> GetClubNoticeAsync(int pageNo, string clubId = "", string clubCode = "");
        Task<(HttpStatusCode, CommonApiResponse)> GetGalleryPhotoAsync(string clubId = "", string clubCode = "");
        #region Subdomain validation
        Task<CommonDBResponse> ValidateClubSubDomain(string subDomainName, string clubCode);
        #endregion
    }
}