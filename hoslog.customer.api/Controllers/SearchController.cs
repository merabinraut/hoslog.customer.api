using Asp.Versioning;
using hoslog.customer.api.Filters;
using hoslog.customer.api.Library;
using hoslog.customer.business.Search;
using hoslog.customer.shared;
using hoslog.customer.shared.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace hoslog.customer.api.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1.0")]
    public class SearchController : BaseApiController
    {
        private ISearchBusiness _searchBusiness;
        public SearchController(ISearchBusiness searchBusiness)
        {
            _searchBusiness = searchBusiness;
        }
        /// <summary>
        /// Used to get club list by location Id
        /// </summary>
        /// <param name="locationId">ID of location </param>
        /// <param name="pageNo">Page number use for pagination</param>
        /// <returns></returns>
        //[LogRequest]
        //[LogResponse]
        [HttpGet, Route("search-by-location")]
        //[SwaggerOperation("", "Used to get club list by location Id", OperationId = "search-by-location")]
        public async Task<IActionResult> SearchByLocation([Required] string locationId, int pageNo)
        {
            AppSettingData appSettingData = new AppSettingData();
            var awsSettingModel = appSettingData.GetAmazonSetting;
            var mappedAwsSetting = awsSettingModel.MapObject<AmazonSettingsCommon>();
            var (statusCode, response) = await _searchBusiness.SearchByLocationAsync(locationId, pageNo, mappedAwsSetting);
            return HandleResponseFromStatusCode(statusCode, response);
        }
        /// <summary>
        /// Used to get club list seach by club particular
        /// </summary>
        /// <param name="searchFilter">Seach by preference like club name</param>
        /// <param name="locationId">ID of location</param>
        /// <param name="clubCategory">Category of the club</param>
        /// <param name="price">Club price range</param>
        /// <param name="shift">Shift morning and afternoon is 1 ,night is 2 and specify the time is 3 = should be insert time</param>
        /// <param name="clubAvailability"></param>
        /// <param name="customerId"></param>
        /// <param name="time">If shift is 3 then time should be insert</param>
        /// <param name="pageNo">Page number for pagination</param>
        /// <returns></returns>
        /// 
        //[LogRequest]
        //[LogResponse]
        [HttpGet, Route("search-by-particular")]
        //[SwaggerOperation("", "Used to get club list by particulars", OperationId = "search-by-particular")]
        public async Task<IActionResult> SearchByParticular(string searchFilter = "", string locationId = "", string clubCategory = "", string price = "", string shift = "", string clubAvailability = "", string customerId = "", string time = "", int pageNo = 0)
        {
            AppSettingData appSettingData = new AppSettingData();
            var awsSettingModel = appSettingData.GetAmazonSetting;
            var mappedAwsSetting = awsSettingModel.MapObject<AmazonSettingsCommon>();
            var (statusCode, response) = await _searchBusiness.SearchByParticularAsync(locationId, searchFilter, price, shift, time, pageNo, customerId, clubAvailability, mappedAwsSetting);
            return HandleResponseFromStatusCode(statusCode, response);
        }

        /// <summary>
        /// Get Host Details by host details 
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="searchByPreference"></param>
        /// <param name="searchByHeight"></param>
        /// <param name="searchByAge"></param>
        /// <param name="searchByBloodType"></param>
        /// <param name="searchByConstellation"></param>
        /// <param name="pageNo"></param>
        /// <returns></returns>
        /// 
        //[LogResponse]
        //[LogRequest]
        [HttpGet, Route("search-by-host-details")]
        //[SwaggerOperation("", "Used to get club list by host details", OperationId = "search-by-host-details")]

        public async Task<IActionResult> SearchByHostDetail(string locationId = "", string searchByPreference = "", string searchByHeight = "", string searchByAge = "", string searchByBloodType = "", string searchByConstellation = "", int pageNo = 0)
        {
            AppSettingData appSettingData = new AppSettingData();
            var awsSettingModel = appSettingData.GetAmazonSetting;
            var mappedAwsSetting = awsSettingModel.MapObject<AmazonSettingsCommon>();
            var (statusCode, response) = await _searchBusiness.GetHostDetailsAsync(locationId, searchByPreference, searchByHeight, searchByAge, searchByBloodType, searchByConstellation, pageNo, mappedAwsSetting);
            return HandleResponseFromStatusCode(statusCode, response);
        }

        //[LogRequest]
        //[LogResponse]
        [AllowAnonymous]
        [HttpGet, Route("search-club")]
        public async Task<IActionResult> SearchClub(string locationId, int minPlanPrice, int maxPlanPrice,int planTime, int noOfPeople = 0, string freeWord = "", string selectedDate = "", string selectedTime = "", string planId = "", string businessHours = "", string tags = "", int pageNo = 1, string vowel = "", bool canVisitStoreNow = false, bool useFreePlanOnly = false)
        {
            var request = new ClubSearchRequestCommon()
            {
                locationId = locationId,
                freeWord = freeWord,
                selectedDate = selectedDate,
                selectedTime = selectedTime,
                noOfPeople = noOfPeople,
                planId = planId,
                businessHours = businessHours,
                tags = tags,
                vowel = vowel,
                pageNo = pageNo,
                canVisitStoreNow = canVisitStoreNow,
                useFreePlanOnly = useFreePlanOnly,
                minPlanPrice = minPlanPrice,
                maxPlanPrice = maxPlanPrice,
                planTime = planTime
            };
            var (statusCode, response) = await _searchBusiness.ClubSearch(request);
            return HandleResponseFromStatusCode(statusCode, response);
        }

        //[LogRequest]
        //[LogResponse]
        [AllowAnonymous]
        [HttpGet, Route("search-host")]
        public async Task<IActionResult> SearchHost(string locationId = "", string freeWord = "", int heightMin = 0, int heightMax = 0, int ageMin = 0, int ageMax = 0, string bloodType = "", string constellation = "", string previousJob = "", string role = "", int pageNo = 1, string vowel = "")
        {
            var request = new HostSearchRequestCommon()
            {
                locationId = locationId,
                freeWord = freeWord,
                heightMin = heightMin,
                heightMax = heightMax,
                ageMin = ageMin,
                ageMax = ageMax,
                bloodType = bloodType,
                constellation = constellation,
                previousJob = previousJob,
                role = role,
                vowel = vowel,
                pageNo = pageNo
            };
            var (statusCode, response) = await _searchBusiness.HostSearch(request);
            return HandleResponseFromStatusCode(statusCode, response);
        }
    }
}

