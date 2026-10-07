using Dapper;
using hoslog.customer.common;
using hoslog.customer.common.Models.ClubManagement;
using hoslog.customer.shared.ClubManagement;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.domain.ClubManagement
{
    public class ClubManagementRepository : IClubManagementRepository
    {
        private readonly RepositoryDao _dao;

        public ClubManagementRepository(IConfiguration configuration)
        {
            _dao = new RepositoryDao(configuration);

        }

        public async Task<IList<ClubBasicInformationModel>> GetClubBasicInformation(string? userId, string clubAgentId = "", string clubCode = "", string timeZoneOffsetValue = "")
        {
            var param = new DynamicParameters();
            string sp_name = "[dbo].[apiproc_cp_club_detail]";
            param.Add("@userId", userId);
            param.Add("@clubId", clubAgentId);
            param.Add("@clubCode", clubCode);
            param.Add("@timeZoneOffsetValue", timeZoneOffsetValue);
            var dbResponse = await _dao.ExecuteAsyncList<ClubBasicInformationModel>(sp_name, param);
            return dbResponse;
        }

        public async Task<List<ClubDetailCommon>> GetClubDetailList(ClubDetailModel objClubDetailModel)
        {
            string proc = "apiproc_cp_club_list";
            var parameters = new DynamicParameters();
            parameters.Add("@searchFilter", objClubDetailModel.search);
            parameters.Add("@pageNo", objClubDetailModel.pageNo);
            var dbResp = await _dao.ExecuteAsyncList<ClubDetailCommon>(proc, parameters);
            return dbResp;
        }

        public async Task<IList<ClubOwnPlanDetailModel>?> GetClubOwnPlanDetails(string? clubId)
        {
            var param = new DynamicParameters();
            string sp_name = "[dbo].[apiproc_cp_club_own_plan_details]";
            param.Add("@clubId", clubId);
            var dbResponse = await _dao.ExecuteAsyncList<ClubOwnPlanDetailModel>(sp_name, param);
            return dbResponse;
        }

        public async Task<IList<ClubPlanDetailModel>> GetClubPlanDetails(string? clubId)
        {
            var param = new DynamicParameters();
            string sp_name = "[dbo].[apiproc_cp_club_detail_plan]";
            param.Add("@clubId", clubId);
            var dbResponse = await _dao.ExecuteAsyncList<ClubPlanDetailModel>(sp_name, param);
            return dbResponse;
        }

        public async Task<List<ClubReviewsCommon>> GetClubReviewAndRatings(string clubAgentId = "", string clubCode = "", int pageNo = 1)
        {
            var response = new List<ClubReviewsCommon>();
            string sp_name = "[dbo].[apiproc_cp_club_review_and_rating]";
            var param = new DynamicParameters();
            param.Add("@flag", "gctr");//for get club top review
            param.Add("@clubId", clubAgentId);
            param.Add("@clubCode", clubCode);
            param.Add("@pageNo", pageNo);
            var dbResponse = await _dao.ExecuteAsyncList<ClubReviewsCommon>(sp_name, param);
            if (dbResponse != null && dbResponse.Count > 0)
            {
                response = dbResponse.MapObjects<ClubReviewsCommon>();
                foreach (var item in response)
                {
                    string sp_name_1 = "[dbo].[apiproc_cp_club_review_and_rating]";
                    var param_1 = new DynamicParameters();
                    param_1.Add("@flag", "ghlfr");// for get host list from review
                    param_1.Add("@clubId", clubAgentId);
                    param_1.Add("@clubCode", clubCode);
                    param_1.Add("@reviewId", item.reviewId);
                    var dbResponse1 = await _dao.ExecuteAsyncList<ClubReviewHostCommon>(sp_name_1, param_1);
                    if (dbResponse1 != null)
                    {
                        item.getClubReviewHostList = dbResponse1.MapObjects<ClubReviewHostCommon>();
                    }
                    string sp_name_2 = "[dbo].[apiproc_cp_club_review_and_rating]";
                    var param_2 = new DynamicParameters();
                    param_2.Add("@flag", "grlfr");// for get remark list from review
                    param_2.Add("@clubId", clubAgentId);
                    param_2.Add("@clubCode", clubCode);
                    param_2.Add("@reviewId", item.reviewId);
                    var dbResponse2 = await _dao.ExecuteAsyncList<ClubReviewRemarkCommon>(sp_name_2, param_2);
                    if (dbResponse2 != null)
                    {
                        item.getClubReviewRemarkList = dbResponse2.MapObjects<ClubReviewRemarkCommon>();
                    }

                    string sp_name_3 = "[dbo].[apiproc_cp_club_review_and_rating]";
                    var param_3 = new DynamicParameters();
                    param_3.Add("@flag", "grcm");// for get remark list from review
                    param_3.Add("@clubId", clubAgentId);
                    param_3.Add("@clubCode", clubCode);
                    param_3.Add("@reviewId", item.intent);
                    var dbResponse3 = await _dao.ExecuteAsyncList<IntentRemarks>(sp_name_3, param_3);
                    if (dbResponse3 != null)
                    {
                        item.getIntentRemarks = dbResponse3.MapObjects<IntentRemarks>();
                    }

                    string sp_name_4 = "[dbo].[apiproc_cp_club_review_and_rating]";
                    var param_4 = new DynamicParameters();
                    param_4.Add("@flag", "qnasr");// get qna smily remarks
                    param_4.Add("@clubId", clubAgentId);
                    param_4.Add("@clubCode", clubCode);
                    param_4.Add("@reviewId", item.reviewId);
                    var dbResponse4 = await _dao.ExecuteAsyncList<QNARemarks>(sp_name_4, param_4);
                    if (dbResponse4 != null)
                    {
                        item.qnaRemarks = dbResponse4.MapObjects<QNARemarks>();
                    }
                }

            }
            return response;
        }

        public async Task<List<ClubScheduleCommon>> GetClubWeeklySchedule(string agentId = "", string clubCode = "", int duration = 1)
        {
            var parameters = new DynamicParameters(new
            {
                agentId,
                clubCode,
                duration
            });
            string proc = "apiproc_get_club_weekly_schedule";
            return await _dao.ExecuteAsyncList<ClubScheduleCommon>(proc, parameters);
        }

        #region shops you can vist now
        public async Task<List<ClubsYouCanVisitNowResponseCommon>> ShopsYouCanVisitNow(ClubsYouCanVisitNowRequestCommon request)
        {
            var parameters = new DynamicParameters(new
            {
                request.locationId,
                request.pageNo,
                request.timeZoneOffsetValue
            });

            string proc = "dbo.apiproc_cp_shops_you_can_visit_now";
            return await _dao.ExecuteAsyncList<ClubsYouCanVisitNowResponseCommon>(proc, parameters);
        }
        #endregion
    }
}
