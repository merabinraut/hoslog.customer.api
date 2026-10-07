using hoslog.customer.common.Models;
using hoslog.customer.common.Models.ReviewModel;
using hoslog.customer.shared;
using System.Net;

namespace hoslog.customer.business.Review
{
    public interface IReviewBusiness
    {
        Task<(HttpStatusCode, object)> GetUserReviewList(PaginationRequestModel objPaginationRequestModel, string userId, AmazonSettingsCommon common, string type = "all");
        Task<(HttpStatusCode, object)> GetReservationDetails(string reservationId, string userId, AmazonSettingsCommon mappedAwsSetting);
        Task<(HttpStatusCode, object)> GetReviewHostListViaClubId(AmazonSettingsCommon mappedAwsSetting, string clubId);
        Task<(HttpStatusCode, object)> GetReviewRemarkList(string clubId, AmazonSettingsCommon mappedAwsSetting);
        Task<(HttpStatusCode, object)> GetReviewQNAList(string clubId, AmazonSettingsCommon mappedAwsSetting);
        Task<(HttpStatusCode, object)> GetReviewDetail(string clubId, AmazonSettingsCommon mappedAwsSetting);
        Task<(HttpStatusCode, object)> ManageReview(ManageReviewRequestModel requestModel);
    }
}
