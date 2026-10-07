using Dapper;
using hoslog.customer.common;
using hoslog.customer.common.Models.Multitenant;
using hoslog.customer.shared;
using hoslog.customer.shared.Multitenant;
using hoslog.customer.shared.Multitenant.ClubInformationMultitenantCommon;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.domain.Multitenant
{
    public class ClubInformationMultitenantRepository : IClubInformationMultitenantRepository
    {
        private readonly RepositoryDao _dao;
        public ClubInformationMultitenantRepository(IConfiguration configuration)
        {
            _dao = new RepositoryDao(configuration);
        }

        public async Task<ClubBasicInformationMultitenantCommon> GetClubBasicInfomration(string subDomainName = "", string clubCode = "", string clubId = "", string timeZoneOffsetValue = "")
        {
            string proc = "dbo.apiproc_multitenant_cp_club_detail";
            var param = new DynamicParameters();
            param.Add("@subDomainName", subDomainName);
            param.Add("@clubCode", clubCode);
            param.Add("@clubId", clubId);
            param.Add("@timeZoneOffsetValue", timeZoneOffsetValue);
            return await _dao.ExecuteAsync<ClubBasicInformationMultitenantCommon>(proc, param);
        }

        public async Task<List<ClubPlanDetailMultitenantCommon>> GetClubPlanDetails(string clubId)
        {
            string proc = "dbo.apiproc_multitenant_cp_club_detail_plan";
            var param = new DynamicParameters();
            param.Add("@clubId", clubId);
            return await _dao.ExecuteAsyncList<ClubPlanDetailMultitenantCommon>(proc, param);
        }

        public async Task<List<ClubReviewsMultitenantCommon>> GetClubReviewAndRatings(string subDomainName, string clubAgentId = "", string clubCode = "", int pageNo = 0)
        {
            var response = new List<ClubReviewsMultitenantCommon>();
            string sp_name = "[dbo].[apiproc_multitenant_cp_club_review_and_rating]";
            var param = new DynamicParameters();
            param.Add("@flag", "gctr");//for get club top review
            param.Add("@subDomainName", subDomainName);
            param.Add("@clubId", clubAgentId);
            param.Add("@clubCode", clubCode);
            param.Add("@pageNo", pageNo);
            var dbResponse = await _dao.ExecuteAsyncList<ClubReviewsMultitenantCommon>(sp_name, param);
            if (dbResponse != null && dbResponse.Count > 0)
            {
                response = dbResponse.MapObjects<ClubReviewsMultitenantCommon>();
                foreach (var item in response)
                {
                    string sp_name_1 = "[dbo].[apiproc_multitenant_cp_club_review_and_rating]";
                    var param_1 = new DynamicParameters();
                    param_1.Add("@flag", "ghlfr");// for get host list from review
                    param_1.Add("@subDomainName", subDomainName);
                    param_1.Add("@clubId", clubAgentId);
                    param_1.Add("@clubCode", clubCode);
                    param_1.Add("@reviewId", item.reviewId);
                    var dbResponse1 = await _dao.ExecuteAsyncList<ClubReviewHostMultitenantCommon>(sp_name_1, param_1);
                    if (dbResponse1 != null)
                    {
                        item.getClubReviewHostList = dbResponse1.MapObjects<ClubReviewHostMultitenantCommon>();
                    }
                    string sp_name_2 = "[dbo].[apiproc_multitenant_cp_club_review_and_rating]";
                    var param_2 = new DynamicParameters();
                    param_2.Add("@flag", "grlfr");// for get remark list from review
                    param_2.Add("@subDomainName", subDomainName);
                    param_2.Add("@clubId", clubAgentId);
                    param_2.Add("@clubCode", clubCode);
                    param_2.Add("@reviewId", item.reviewId);
                    var dbResponse2 = await _dao.ExecuteAsyncList<ClubReviewRemarkMultitenantCommon>(sp_name_2, param_2);
                    if (dbResponse2 != null)
                    {
                        item.getClubReviewRemarkList = dbResponse2.MapObjects<ClubReviewRemarkMultitenantCommon>();
                    }

                    string sp_name_3 = "[dbo].[apiproc_multitenant_cp_club_review_and_rating]";
                    var param_3 = new DynamicParameters();
                    param_3.Add("@flag", "grcm");// for get remark list from review
                    param_3.Add("@subDomainName", subDomainName);
                    param_3.Add("@clubId", clubAgentId);
                    param_3.Add("@clubCode", clubCode);
                    param_3.Add("@reviewId", item.intent);
                    var dbResponse3 = await _dao.ExecuteAsyncList<IntentRemarksMultitenant>(sp_name_3, param_3);
                    if (dbResponse3 != null)
                    {
                        item.getIntentRemarks = dbResponse3.MapObjects<IntentRemarksMultitenant>();
                    }

                    string sp_name_4 = "[dbo].[apiproc_multitenant_cp_club_review_and_rating]";
                    var param_4 = new DynamicParameters();
                    param_4.Add("@flag", "qnasr");// get qna smily remarks
                    param_4.Add("@subDomainName", subDomainName);
                    param_4.Add("@clubId", clubAgentId);
                    param_4.Add("@clubCode", clubCode);
                    param_4.Add("@reviewId", item.reviewId);
                    var dbResponse4 = await _dao.ExecuteAsyncList<QNARemarksMultitenant>(sp_name_4, param_4);
                    if (dbResponse4 != null)
                    {
                        item.qnaRemarks = dbResponse4.MapObjects<QNARemarksMultitenant>();
                    }
                }

            }
            return response;
        }

        public async Task<List<ClubScheduleMultitenantCommon>> GetClubWeeklySchedule(string subDomainName = "", string agentId = "", string clubCode = "", int duration = 1)
        {
            string proc = "dbo.apiproc_multitenant_get_club_weekly_schedule";
            var parameters = new DynamicParameters(new
            {
                subDomainName,
                agentId,
                clubCode,
                duration
            });
            return await _dao.ExecuteAsyncList<ClubScheduleMultitenantCommon>(proc, parameters);
        }

        public async Task<List<ScheduleResponseMultitenantCommon>> GetScheduleListAsync(ScheduleDetailsRequestMultitenantCommon requesMapped)
        {
            string proc = "dbo.apiproc_multitenant_cp_get_schedule_details ";
            var parameters = new DynamicParameters();
            parameters.Add("@subDomainName", requesMapped.subDomainName);
            parameters.Add("@agentId", requesMapped.clubId);
            parameters.Add("@clubCode", requesMapped.clubCode);
            parameters.Add("@filterDate", requesMapped.date);
            parameters.Add("@timeZoneOffsetValue", requesMapped.timeZoneOffsetValue);
            var dbResp = await _dao.ExecuteAsyncList<ScheduleResponseMultitenantCommon>(proc, parameters);
            return dbResp;
        }

        public async Task<List<NoticeDetailResponseMultitenantCommon>> GetClubNoticeAsync(NoticeDetailRequestMultitenantCommon request)
        {
            string proc = "dbo.apiproc_multitenant_cp_get_notice_list ";
            var parameters = new DynamicParameters();
            parameters.Add("@subDomainName", request.subDomainName);
            parameters.Add("@clubId", request.clubId);
            parameters.Add("@pageNo", request.pageNo);
            parameters.Add("@clubCode", request.clubCode);
            var dbResp = await _dao.ExecuteAsyncList<NoticeDetailResponseMultitenantCommon>(proc, parameters);
            return dbResp;
        }

        public async Task<List<PhotoGalleryResponseMultitenantCommon>> GetGalleryPhotoAsync(PhotoGalleryRequestMultitenantCommon requestModel)
        {
            string proc = "dbo.apiproc_multitenant_cp_photo_galary_list ";
            var parameters = new DynamicParameters();
            parameters.Add("@subDomainName", requestModel.subDomainName);
            parameters.Add("@agentId", requestModel.clubId);
            parameters.Add("@clubCode", requestModel.clubCode);
            return await _dao.ExecuteAsyncList<PhotoGalleryResponseMultitenantCommon>(proc, parameters);
        }

        #region Subdomain validation
        public async Task<CommonDBResponse> ValidateClubSubDomain(string subDomainName, string clubCode)
        {
            string proc = "dbo.apiproc_multitenant_cp_validate_club_subdomain ";
            var parameters = new DynamicParameters();
            parameters.Add("@subDomainName", subDomainName);
            parameters.Add("@clubCode", clubCode);
            return await _dao.ExecuteAsync<CommonDBResponse>(proc, parameters);
        }

        public async Task<List<ClubSpecificPlanDetailModel>> GetClubOwnPlanDetails(string clubId)
        {
            var param = new DynamicParameters();
            string sp_name = "[dbo].[apiproc_cp_club_own_plan_details]";
            param.Add("@clubId", clubId);
            var dbResponse = await _dao.ExecuteAsyncList<ClubSpecificPlanDetailModel>(sp_name, param);
            return dbResponse;
        }
        #endregion
    }
}