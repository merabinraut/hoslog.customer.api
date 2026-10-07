using Dapper;
using hoslog.customer.common.Models;
using hoslog.customer.shared;
using hoslog.customer.shared.Review;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.domain.Review
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly RepositoryDao _dao;

        public ReviewRepository(IConfiguration configuration)
        {
            _dao = new RepositoryDao(configuration);
        }

        public async Task<IList<ReviewClubShortDetailModel>> GetClubShortDetail(string? clubId)
        {
            string sp_name = "[dbo].[apiproc_review_club_short_detail]";
            var param = new DynamicParameters();
            param.Add("@clubId", clubId);
            var dbResponse = await _dao.ExecuteAsyncList<ReviewClubShortDetailModel>(sp_name, param);
            return dbResponse;
        }

        public async Task<IList<ReviewReservationResponseModel>> GetReservationDetail(ReviewReservationRequestModel dbRequest)
        {
            string sp_name = "[dbo].[apiproc_cp_review_reservation_detail]";
            var param = new DynamicParameters();
            param.Add("@userId", dbRequest.userId);
            param.Add("@reservationId", dbRequest.reservationId);
            var dbResponse = await _dao.ExecuteAsyncList<ReviewReservationResponseModel>(sp_name, param);
            return dbResponse;
        }

        public async Task<IList<ReviewDichotomousAnswerModel>> GetReviewDichotomousAnswerList()
        {
            string sp_name = "[dbo].[apiproc_cp_review_dichotomous_answer_list]";
            var param = new DynamicParameters();
            var dbResponse = await _dao.ExecuteAsyncList<ReviewDichotomousAnswerModel>(sp_name, param);
            return dbResponse;
        }

        public async Task<IList<ReviewDichotomousQuestionModel>> GetReviewDichotomousQuestionList()
        {
            string sp_name = "[dbo].[apiproc_cp_review_dichotomous_question_list]";
            var param = new DynamicParameters();
            var dbResponse = await _dao.ExecuteAsyncList<ReviewDichotomousQuestionModel>(sp_name, param);
            return dbResponse;
        }

        public async Task<IList<ReviewHostListModel>> GetReviewHostListViaClubId(string decryptClubId)
        {
            string sp_name = "[dbo].[apiproc_cp_review_hostlist]";
            var param = new DynamicParameters();
            param.Add("@clubId", decryptClubId);
            var dbResponse = await _dao.ExecuteAsyncList<ReviewHostListModel>(sp_name, param);
            return dbResponse;
        }

        public async Task<IList<ReviewRemarkListModel>> GetReviewRemarkList()
        {
            string sp_name = "[dbo].[apiproc_cp_review_remarklist]";
            var param = new DynamicParameters();
            var dbResponse = await _dao.ExecuteAsyncList<ReviewRemarkListModel>(sp_name, param);
            return dbResponse;
        }

        public async Task<List<ReviewCommon>> GetUserReviewList(PaginationRequestModel objPaginationRequestModel, string userId, string type = "all")
        {
            string proc = "apiproc_cp_review_management";
            var parameters = new DynamicParameters();
            parameters.Add("@flag", "gcrl");
            parameters.Add("@userId", userId);
            parameters.Add("@pageNo", objPaginationRequestModel.pageNo);
            parameters.Add("@type", type);
            var dbResp = await _dao.ExecuteAsyncList<ReviewCommon>(proc, parameters);
            return dbResp;

            //throw new NotImplementedException();
        }

        public async Task<CommonDBResponse> ManageReview(ReviewRequestModel dbRequest)
        {
            string sp_name = "[dbo].[apiproc_cp_manage_review]";
            var param = new DynamicParameters();
            param.Add("@clubId", dbRequest.clubId);
            param.Add("@userId", dbRequest.userId);
            param.Add("@reservationId", dbRequest.reservationId);
            param.Add("@hostList", dbRequest.hostList);
            param.Add("@mvpHostId", dbRequest.mvpHostId);
            param.Add("@dichotomousList", dbRequest.dichotomousList);
            param.Add("@remarkList", dbRequest.remarkList);
            param.Add("@ratingScale", dbRequest.ratingScale);
            param.Add("@Intent", dbRequest.intent);
            param.Add("@remark", dbRequest.remark);
            param.Add("@actionUser", null);
            param.Add("@actionPlatform", null);
            param.Add("@actionIP", null);
            var dbResponse = await _dao.ExecuteAsync<CommonDBResponse>(sp_name, param);
            return dbResponse;
        }
    }
}
