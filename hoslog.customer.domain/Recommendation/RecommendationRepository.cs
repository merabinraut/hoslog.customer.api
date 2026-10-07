using Dapper;
using hoslog.customer.common.Models;
using hoslog.customer.common.Models.RecommendationModel;
using hoslog.customer.shared.Recommendation;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.domain.Recommendation;

public class RecommendationRepository : IRecommendationRepository
{
    private readonly RepositoryDao _dao;
    public RecommendationRepository(IConfiguration configuration)
    {
        _dao = new RepositoryDao(configuration);
    }
    #region RECOMMENDATION HOST AND CLUB FOR OWL CARASOUL
    public async Task<List<RecommendationClubModelCommon>> GetRecommendationClubList(string userId)
    {
        string sp_name = "apiproc_cp_get_recommendation_club_list";
        var param = new DynamicParameters();
        param.Add("@userId", userId);
        var dbResponse = await _dao.ExecuteAsyncList<RecommendationClubModelCommon>(sp_name, param);
        return dbResponse;
    }

    public async Task<List<RecommendationHostModelCommon>> GetRecommendationHostList(string userId)
    {
        string sp_name = "apiproc_cp_get_recommendation_host_list";
        var param = new DynamicParameters();
        param.Add("@userId", userId);
        var dbResponse = await _dao.ExecuteAsyncList<RecommendationHostModelCommon>(sp_name, param);
        return dbResponse;

    }
    #endregion

    #region RECOMMENDATION HOST AND CLUB FOR HOME PAGE TAB
    public async Task<IList<RecommendationClubCommonModel>> GetHomepageRecommendationClubList(string? userId, string timeZoneOffsetValue = "")
    {
        string sp_name = "[dbo].[apiproc_cp_get_homepage_recommended_club]";
        var param = new DynamicParameters();
        param.Add("@userId", userId);
        param.Add("@locationId", null);
        param.Add("@timeZoneOffsetValue", timeZoneOffsetValue);
        var dbResponse = await _dao.ExecuteAsyncList<RecommendationClubCommonModel>(sp_name, param);
        foreach (var item in dbResponse)
        {
            #region REVIEW AND RATING VIA CLUBID
            string sp_name_rnr = "[dbo].[apiproc_cp_club_review_and_rating_detail]";
            var param_rnr = new DynamicParameters();
            param_rnr.Add("@clubId", item.clubId);
            var dbRnrResponse = await _dao.ExecuteAsyncList<RecommendationClubCommonModel>(sp_name_rnr, param_rnr);
            if (dbRnrResponse != null)
            {
                //item.averageRating = Convert.ToInt32(dbRnrResponse[0].averageRating);
                //item.totalComment = Convert.ToInt32(dbRnrResponse[0].totalComment);
                item.averageRating = dbRnrResponse[0].averageRating;
                item.totalComment = dbRnrResponse[0].totalComment;
            }
            #endregion

            #region CLUB HOST IMAGE 
            List<HostImageForClubCard> hostImage = new List<HostImageForClubCard>();
            string sp_name_hi = "[dbo].[apiproc_cp_get_host_for_club]";
            var host_param = new DynamicParameters();
            host_param.Add("@clubId", item.clubId);
            var dbHostImageReponse = await _dao.ExecuteAsyncList<HostImageForClubCard>(sp_name_hi, host_param);
            foreach (var hostImage_item in dbHostImageReponse)
            {
                hostImage.Add(hostImage_item);
            }
            item.hostGalleryImage = hostImage;
            #endregion
        }
        return dbResponse;
    }
    public async Task<IList<RecommendationHostCommonModel>> GetHomepageRecommendationHostList(string? userId)
    {
        string sp_name = "[dbo].[apiproc_cp_get_homepage_recommended_host_list]";
        var param = new DynamicParameters();
        param.Add("@userId", userId);
        param.Add("@locationId", null);
        var dbResponse = await _dao.ExecuteAsyncList<RecommendationHostCommonModel>(sp_name, param);
        return dbResponse;
    }
    #endregion

    #region RECOMMENDATION HOST AND CLUB FOR MAIN PAGE TAB
    public async Task<List<RecommendedClubCommon>> GetMainpageRecommendedClubList(string? userId, string? locationId, int? positionId, string timeZoneOffsetValue = "")
    {
        const string clubListProc = "[dbo].[apiproc_cp_get_mainpage_recommended_club_list]";
        var clubParams = new DynamicParameters();
        clubParams.Add("@userId", userId);
        clubParams.Add("@locationId", locationId);
        clubParams.Add("@positionId", positionId);
        clubParams.Add("@timeZoneOffsetValue", timeZoneOffsetValue);

        var recommendedClubs = await _dao.ExecuteAsyncList<RecommendedClubCommon>(clubListProc, clubParams);

        if (recommendedClubs is null || recommendedClubs.Count == 0)
            return recommendedClubs ?? new List<RecommendedClubCommon>();

        var clubIds = recommendedClubs.Select(c => c.clubId).Distinct();

        const string hostImageProc = "[dbo].[apiproc_cp_get_host_for_club]";
        var hostParams = new DynamicParameters();
        hostParams.Add("@clubId", string.Join(",", clubIds));

        var hostImages = await _dao.ExecuteAsyncList<HostImageForClubCardCommon>(hostImageProc, hostParams);

        var hostImageDict = hostImages
            .GroupBy(h => h.clubId)
            .ToDictionary(g => g.Key, g => (IList<HostImageForClubCardCommon>)g.ToList());

        foreach (var club in recommendedClubs)
        {
            club.hostGalleryImage = hostImageDict.TryGetValue(club.clubId, out var images) ? images.ToList() : new List<HostImageForClubCardCommon>();
        }

        return recommendedClubs;
    }

    public async Task<RecommendedClubPlanCommon> GetRecommendedClubPlan(string clubId)
    {
        string procedure = "dbo.apiproc_cp_get_recommended_club_plan";
        var parameters = new DynamicParameters(new
        {
            clubId
        });
        return await _dao.ExecuteAsync<RecommendedClubPlanCommon>(procedure, parameters);
    }

    public async Task<List<RecommendedHostCommon>> GetMainpageRecommendedHostList(string? userId, string? locationId, int? positionId)
    {
        string sp_name = "[dbo].[apiproc_cp_get_mainpage_recommended_host_list]";
        var param = new DynamicParameters();
        param.Add("@userId", userId);
        param.Add("@locationId", locationId);
        param.Add("@positionId", positionId);
        var dbResponse = await _dao.ExecuteAsyncList<RecommendedHostCommon>(sp_name, param);
        return dbResponse;
    }

    public async Task<IList<RecommendationClubCommonModel>> GetMainpageRecommendationClubList(string? userId, string? locationId, int? positionId)
    {
        const string clubListProc = "[dbo].[apiproc_cp_get_mainpage_recommended_club_list]";
        var clubParams = new DynamicParameters();
        clubParams.Add("@userId", userId);
        clubParams.Add("@locationId", locationId);
        clubParams.Add("@positionId", positionId);

        var recommendedClubs = await _dao.ExecuteAsyncList<RecommendationClubCommonModel>(clubListProc, clubParams);

        if (recommendedClubs is null || recommendedClubs.Count == 0)
            return recommendedClubs ?? new List<RecommendationClubCommonModel>();

        var clubIds = recommendedClubs.Select(c => c.clubId).Distinct();

        const string hostImageProc = "[dbo].[apiproc_cp_get_host_for_club]";
        var hostParams = new DynamicParameters();
        hostParams.Add("@clubId", string.Join(",", clubIds));

        var hostImages = await _dao.ExecuteAsyncList<HostImageForClubCard>(hostImageProc, hostParams);

        var hostImageDict = hostImages
            .GroupBy(h => h.clubId)
            .ToDictionary(g => g.Key, g => (IList<HostImageForClubCard>)g.ToList());

        foreach (var club in recommendedClubs)
        {
            club.hostGalleryImage = hostImageDict.TryGetValue(club.clubId, out var images) ? images.ToList() : new List<HostImageForClubCard>();
        }

        return recommendedClubs;
    }

    public async Task<IList<RecommendationHostCommonModel>> GetMainpageRecommendationHostList(string? userId, string? locationId, int? positionId)
    {
        string sp_name = "[dbo].[apiproc_cp_get_mainpage_recommended_host_list]";
        var param = new DynamicParameters();
        param.Add("@userId", userId);
        param.Add("@locationId", locationId);
        param.Add("@positionId", positionId);
        var dbResponse = await _dao.ExecuteAsyncList<RecommendationHostCommonModel>(sp_name, param);
        return dbResponse;
    }
    #endregion
    public async Task<List<EditorsRecommendationClubCommon>> GetEditorsRecommendationClubList(string locationId, int? pageNo, int? pageSize)
    {
        string sp = "dbo.apiproc_cp_get_editors_recommendation_club_list";
        var parameters = new DynamicParameters();
        parameters.Add("@locationId", locationId);
        parameters.Add("@pageNo", pageNo);
        parameters.Add("@pageSize", pageSize);
        return await _dao.ExecuteAsyncList<EditorsRecommendationClubCommon>(sp, parameters);
    }

    public async Task<List<EditorsRecommendationClubDetailCommon>> GetEditorsRecommendationClubDetailList(string userId, string? locationId, int? pageNo, int? pageSize,
        string? timeZoneOffsetValue)
    {
        string sp="dbo.apiproc_cp_editors_recommendation_club_detail_list";
        var parameters = new DynamicParameters();
        parameters.Add("@userId", userId);
        parameters.Add("@locationId", locationId);
        parameters.Add("@pageNo", pageNo ?? 1);
        parameters.Add("@pageSize", pageSize ?? 10);
        parameters.Add("@timeZoneOffsetValue", timeZoneOffsetValue);
        var recommendedClubs = await _dao.ExecuteAsyncList<EditorsRecommendationClubDetailCommon>(sp, parameters);
        
        if (recommendedClubs[0].code !="0" || recommendedClubs is null || recommendedClubs.Count == 0)
            return recommendedClubs ?? new List<EditorsRecommendationClubDetailCommon>();

        var clubIds = recommendedClubs.Select(c => c.clubId).Distinct();

        const string hostImageProc = "[dbo].[apiproc_cp_get_host_for_club]";
        var hostParams = new DynamicParameters();
        hostParams.Add("@clubId", string.Join(",", clubIds));

        var hostImages = await _dao.ExecuteAsyncList<HostImageForClubCardCommon>(hostImageProc, hostParams);

        var hostImageDict = hostImages
            .GroupBy(h => h.clubId)
            .ToDictionary(g => g.Key, g => (IList<HostImageForClubCardCommon>)g.ToList());

        foreach (var club in recommendedClubs)
        {
            club.hostGalleryImage = hostImageDict.TryGetValue(club.clubId, out var images) ? images.ToList() : new List<HostImageForClubCardCommon>();
        }

        return recommendedClubs;
    }
}
