using hoslog.customer.common.Models.RecommendationModel;
using hoslog.customer.shared;
using hoslog.customer.shared.Search;
using System.Net;

namespace hoslog.customer.business.Search
{
    public interface ISearchBusiness
    {
        Task<(HttpStatusCode, object)> GetHostDetailsAsync(string locationId, string searchByPreference, string searchByHeight, string searchByAge, string searchByBloodType, string searchByConstellation, int pageNo, AmazonSettingsCommon mappedAwsSetting);
        Task<(HttpStatusCode, object)> SearchByLocationAsync(string locationId, int pageNo, AmazonSettingsCommon amazonSettingsCommon);
        Task<(HttpStatusCode, object)> SearchByParticularAsync(string locationId, string searchFilter, string price, string shift, string time, int pageNo, string customerId, string clubAvailability, AmazonSettingsCommon mappedAwsSetting);
        Task<List<HostImageForClubCard>> GetHostImageViaClubId(string clubId);
        Task<List<ClubSearchPlanCommon>> GetClubPlan(string clubId);
        Task<(HttpStatusCode, object)> ClubSearch(ClubSearchRequestCommon request);
        Task<(HttpStatusCode, object)> HostSearch(HostSearchRequestCommon request);
    }
}
