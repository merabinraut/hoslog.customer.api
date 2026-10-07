using hoslog.customer.common.Models;
using hoslog.customer.common.Models.RecommendationModel;
using hoslog.customer.shared.Recommendation;

namespace hoslog.customer.domain.Recommendation;

public interface IRecommendationRepository
{
    Task<IList<RecommendationClubCommonModel>> GetHomepageRecommendationClubList(string? userId, string timeZoneOffsetValue = "");
    Task<IList<RecommendationHostCommonModel>> GetHomepageRecommendationHostList(string? userId);
    Task<IList<RecommendationClubCommonModel>> GetMainpageRecommendationClubList(string? userId, string? locationId, int? positionId);
    Task<IList<RecommendationHostCommonModel>> GetMainpageRecommendationHostList(string? userId, string? locationId, int? positionId);
    Task<List<RecommendationClubModelCommon>> GetRecommendationClubList(string userId);
    Task<List<RecommendationHostModelCommon>> GetRecommendationHostList(string userId);
    Task<List<RecommendedClubCommon>> GetMainpageRecommendedClubList(string? userId, string? locationId, int? positionId, string timeZoneOffsetValue = "");
    Task<RecommendedClubPlanCommon> GetRecommendedClubPlan(string clubId);
    Task<List<RecommendedHostCommon>> GetMainpageRecommendedHostList(string? userId, string? locationId, int? positionId);
    Task<List<EditorsRecommendationClubCommon>> GetEditorsRecommendationClubList(string locationId, int? pageNo,
        int? pageSize);

    Task<List<EditorsRecommendationClubDetailCommon>> GetEditorsRecommendationClubDetailList(string userId,string locationId,
        int? pageNo, int? pageSize,string? timeZoneOffsetValue);
}
