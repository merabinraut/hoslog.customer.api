using System.Net;
using hoslog.customer.shared;

namespace hoslog.customer.business.Recommendation;

public interface IRecommendationBusiness
{
    Task<(HttpStatusCode, object)> GetHomepageRecommendationClubAndHostList(AmazonSettingsCommon common);
    Task<(HttpStatusCode, object)> GetMainpageRecommendationClubAndHostList(AmazonSettingsCommon mappedAwsSetting, string? locationId, int? positionId);
    Task<(HttpStatusCode, object)> GetRecommendationClubList(AmazonSettingsCommon common);
    Task<(HttpStatusCode, object)> GetRecommendationHostList(AmazonSettingsCommon common);
    Task<(HttpStatusCode, CommonApiResponse)> GetMainPageRecommendedClubList(string locationId, int positionId);
    Task<(HttpStatusCode, CommonApiResponse)> GetMainPageRecommendedHostList(string locationId, int positionId);
    Task<(HttpStatusCode,object)> GetEditorsRecommendationClubList(string locationId, int? pageNo, int? pageSize);

    Task<(HttpStatusCode, object)>
        GetEditorsRecommendationClubDetailList(string locationId, int? pageNo, int? pageSize);
}
