using Dapper;
using hoslog.customer.common.Models.RecommendationModel;
using hoslog.customer.common.Search;
using hoslog.customer.shared.Search;
using Microsoft.Extensions.Configuration;

namespace hoslog.customer.domain.Search
{
    public class SearchRepository : ISearchRepository
    {
        private readonly RepositoryDao _dao;

        public SearchRepository(IConfiguration configuration)
        {
            _dao = new RepositoryDao(configuration);
        }

        public async Task<List<HostDetailsResponseCommon>> SearchByHostDetailsAsync(SearchByHostDetail request)
        {
            string proc = "apiproc_cp_search_by_host_details ";
            var parameters = new DynamicParameters();
            parameters.Add("@locationId", request.locationId);
            parameters.Add("@searchByHeight", request.searchByHeight);
            parameters.Add("@searchByAge", request.searchByAge);
            parameters.Add("@searchByBloodType", request.searchByBloodType);
            parameters.Add("@searchByConstellation", request.searchByConstellation);
            parameters.Add("@searchFilter", request.searchByPreference);
            parameters.Add("@pageNo", request.pageNo);
            var dbResp = await _dao.ExecuteAsyncList<HostDetailsResponseCommon>(proc, parameters);
            return (dbResp);
        }

        public async Task<List<ClubListCommon>> SearchByLocationAsync(SearchByLocationModel request)
        {
            string proc = "apiproc_cp_get_recommended_club_list ";
            var parameters = new DynamicParameters();
            parameters.Add("@locationId", request.locationName);
            parameters.Add("@pageNo", request.pageNo);
            parameters.Add("@timeZoneOffsetValue", request.timeZoneOffsetValue);
            var dbResp = await _dao.ExecuteAsyncList<ClubListCommon>(proc, parameters);
            return (dbResp);
        }

        public async Task<List<ClubListCommon>> SearchByParticularAsync(SearchByParticular request)
        {
            string proc = "apiproc_cp_search_by_preference ";
            var parameters = new DynamicParameters();
            parameters.Add("@locationId", request.locationId);
            parameters.Add("@searchFilter", request.SearchFilter);
            parameters.Add("@clubCategory", request.clubCategory);
            parameters.Add("@price", request.price);
            parameters.Add("@shift", request.shift);
            parameters.Add("@time", request.time);
            parameters.Add("@clubAvailability", request.clubAvailability);
            parameters.Add("@skip", request.pageNo);
            parameters.Add("@customerId", request.customerId);
            var dbResp = await _dao.ExecuteAsyncList<ClubListCommon>(proc, parameters);
            return (dbResp);
        }

        public async Task<List<ClubListCommon>> SearchByPreferenceAsync(SearchByPreference request)
        {
            string proc = "apiproc_cp_search_by_preference ";
            var parameters = new DynamicParameters();
            parameters.Add("@searchFilter", request.searchByPreference);
            parameters.Add("@price", request.fee);
            parameters.Add("@clubAvailability", request.searchByBusinessHour);
            parameters.Add("@clubAvailability", request.shop);
            parameters.Add("@pageNo", request.pageNo);
            parameters.Add("@locationId", request.locationId);
            parameters.Add("@customerId", request.customerId);
            var dbResp = await _dao.ExecuteAsyncList<ClubListCommon>(proc, parameters);
            return (dbResp);
        }

        public async Task<List<ClubSearchResponseCommon>> ClubSearch(ClubSearchRequestCommon request)
        {
            string procedure = "dbo.apiproc_cp_club_filter_management";
            var parameters = new DynamicParameters();
            parameters.Add("@locationId", !string.IsNullOrEmpty(request.locationId) ? request.locationId : null);
            parameters.Add("@searchText", !string.IsNullOrEmpty(request.freeWord) ? request.freeWord : null);
            parameters.Add("@selectedDate", !string.IsNullOrEmpty(request.selectedDate) ? request.selectedDate : null);
            parameters.Add("@selectedTime", !string.IsNullOrEmpty(request.selectedTime) ? request.selectedTime : null);
            parameters.Add("@noOfPeople", request.noOfPeople > 0 ? request.noOfPeople : null);
            parameters.Add("@planId", !string.IsNullOrEmpty(request.planId) ? request.planId : null);
            parameters.Add("@businessHours", !string.IsNullOrEmpty(request.businessHours) ? request.businessHours : null);
            parameters.Add("@tags", !string.IsNullOrEmpty(request.tags) ? request.tags : null);
            parameters.Add("@customerId", !string.IsNullOrEmpty(request.customerId) ? request.customerId : null);
            parameters.Add("@hiraganaStartVowel", !string.IsNullOrEmpty(request.vowel) ? request.vowel : null);
            parameters.Add("@pageNo", request.pageNo);
            parameters.Add("@timeZoneOffsetValue", request.timeZoneOffsetValue);
            parameters.Add("@canVisitStoreNow", request.canVisitStoreNow);
            parameters.Add("@useFreePlanOnly", request.useFreePlanOnly);
            parameters.Add("@minPrice", request.minPlanPrice);
            parameters.Add("@maxPrice", request.maxPlanPrice);
            parameters.Add("@planTime", request.planTime);
            return await _dao.ExecuteAsyncList<ClubSearchResponseCommon>(procedure, parameters);
        }

        public async Task<List<HostImageForClubCard>> GetHostImageViaClubId(string clubId)
        {
            string procedure = "dbo.apiproc_cp_get_host_images_via_club";
            var parameters = new DynamicParameters(new
            {
                clubId
            });
            parameters.Add("@clubId", clubId);
            return await _dao.ExecuteAsyncList<HostImageForClubCard>(procedure, parameters);
        }

        public async Task<List<ClubSearchPlanCommon>> GetClubPlan(string clubId)
        {
            string procedure = "dbo.apiproc_cp_get_club_plan";
            var parameters = new DynamicParameters(new
            {
                clubId
            });
            return await _dao.ExecuteAsyncList<ClubSearchPlanCommon>(procedure, parameters);
        }

        public async Task<List<HostSearchResponseCommon>> HostSearch(HostSearchRequestCommon request)
        {
            string procedure = "dbo.apiproc_cp_host_filter_management";
            var parameters = new DynamicParameters();
            parameters.Add("@locationId", !string.IsNullOrEmpty(request.locationId) ? request.locationId : null);
            parameters.Add("@searchText", !string.IsNullOrEmpty(request.freeWord) ? request.freeWord : null);
            parameters.Add("@heightMin", request.heightMin > 0 ? request.heightMin : null);
            parameters.Add("@heightMax", request.heightMax > 0 ? request.heightMax : null);
            parameters.Add("@ageMin", request.ageMin > 0 ? request.ageMin : null);
            parameters.Add("@ageMax", request.ageMax > 0 ? request.ageMax : null);
            parameters.Add("@bloodType", !string.IsNullOrEmpty(request.bloodType) ? request.bloodType : null);
            parameters.Add("@constellation", !string.IsNullOrEmpty(request.constellation) ? request.constellation : null);
            parameters.Add("@previousJob", !string.IsNullOrEmpty(request.previousJob) ? request.previousJob : null);
            parameters.Add("@role", !string.IsNullOrEmpty(request.role) ? request.role : null);
            parameters.Add("@customerId", !string.IsNullOrEmpty(request.customerId) ? request.customerId : null);
            parameters.Add("@hiraganaStartVowel", !string.IsNullOrEmpty(request.vowel) ? request.vowel : null);
            parameters.Add("@pageNo", request.pageNo);
            return await _dao.ExecuteAsyncList<HostSearchResponseCommon>(procedure, parameters);
        }
    }
}
