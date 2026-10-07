using hoslog.customer.common.Models.Multitenant;
using hoslog.customer.shared;
using hoslog.customer.shared.Multitenant;
using hoslog.customer.shared.Multitenant.ClubInformationMultitenantCommon;

namespace hoslog.customer.domain.Multitenant
{
    public interface IClubInformationMultitenantRepository
    {
        Task<ClubBasicInformationMultitenantCommon> GetClubBasicInfomration(string subDomainName = "", string clubCode = "", string clubId = "", string timeZoneOffsetValue = "");
        Task<List<ClubPlanDetailMultitenantCommon>> GetClubPlanDetails(string clubId);
        Task<List<ClubReviewsMultitenantCommon>> GetClubReviewAndRatings(string subDomainName, string clubAgentId = "", string clubCode = "", int pageNo = 0);
        Task<List<ClubScheduleMultitenantCommon>> GetClubWeeklySchedule(string subDomainName = "", string agentId = "", string clubCode = "", int duration = 1);
        Task<List<ScheduleResponseMultitenantCommon>> GetScheduleListAsync(ScheduleDetailsRequestMultitenantCommon requesMapped);
        Task<List<NoticeDetailResponseMultitenantCommon>> GetClubNoticeAsync(NoticeDetailRequestMultitenantCommon request);
        Task<List<PhotoGalleryResponseMultitenantCommon>> GetGalleryPhotoAsync(PhotoGalleryRequestMultitenantCommon requestModel);
        #region Subdomain validation
        Task<CommonDBResponse> ValidateClubSubDomain(string subDomainName, string clubCode);
        Task<List<ClubSpecificPlanDetailModel>> GetClubOwnPlanDetails(string clubId);
        #endregion
    }
}