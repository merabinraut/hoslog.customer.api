using hoslog.customer.common.Models;
using hoslog.customer.shared;
using hoslog.customer.shared.Review;

namespace hoslog.customer.domain.Review
{
    public interface IReviewRepository
    {
        Task<IList<ReviewClubShortDetailModel>> GetClubShortDetail(string? clubId);
        Task<IList<ReviewReservationResponseModel>> GetReservationDetail(ReviewReservationRequestModel dbRequest);
        Task<IList<ReviewDichotomousAnswerModel>> GetReviewDichotomousAnswerList();
        Task<IList<ReviewDichotomousQuestionModel>> GetReviewDichotomousQuestionList();
        Task<IList<ReviewHostListModel>> GetReviewHostListViaClubId(string decryptClubId);
        Task<IList<ReviewRemarkListModel>> GetReviewRemarkList();
        Task<List<ReviewCommon>> GetUserReviewList(PaginationRequestModel objPaginationRequestModel, string userId, string type = "all");
        Task<CommonDBResponse> ManageReview(ReviewRequestModel dbRequest);
    }
}
