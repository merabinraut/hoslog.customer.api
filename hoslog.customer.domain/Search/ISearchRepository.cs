using hoslog.customer.common.Models.RecommendationModel;
using hoslog.customer.common.Search;
using hoslog.customer.shared.Search;

namespace hoslog.customer.domain.Search
{
    public interface ISearchRepository
    {
        Task<List<HostDetailsResponseCommon>> SearchByHostDetailsAsync(SearchByHostDetail request);
        Task<List<ClubListCommon>> SearchByLocationAsync(SearchByLocationModel request);
        Task<List<ClubListCommon>> SearchByParticularAsync(SearchByParticular request);
        Task<List<ClubListCommon>> SearchByPreferenceAsync(SearchByPreference request);
        Task<List<ClubSearchResponseCommon>> ClubSearch(ClubSearchRequestCommon request);
        Task<List<HostImageForClubCard>> GetHostImageViaClubId(string clubId);
        Task<List<ClubSearchPlanCommon>> GetClubPlan(string clubId);
        Task<List<HostSearchResponseCommon>> HostSearch(HostSearchRequestCommon request);
    }
}
