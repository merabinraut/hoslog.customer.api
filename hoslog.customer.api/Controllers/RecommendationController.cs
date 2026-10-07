using Asp.Versioning;
using hoslog.customer.api.Filters;
using hoslog.customer.api.Library;
using hoslog.customer.business.Recommendation;
using hoslog.customer.shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hoslog.customer.api.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1.0")]
    [AllowAnonymous]
    public class RecommendationController : BaseApiController
    {
        private readonly IRecommendationBusiness _business;

        public RecommendationController(IRecommendationBusiness business)
        {
            _business = business;
        }
        /// <summary>
        /// Get recommendation host list
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /v1/Recommendation/get-recommendation-club-list
        ///     {
        ///        
        ///        
        ///     }
        ///
        /// </remarks>
        //[LogRequest]
        //[LogResponse]
        // [HttpGet, Route("get-recommendation-host-list")]
        // public async Task<IActionResult> GetRecommendationHostList()
        // {
        //     AppSettingData appSettingData = new AppSettingData();
        //     var mappedAwsSetting = appSettingData.MapObject<AmazonSettingsCommon>();
        //     var (statusCode, response) = await _business.GetRecommendationHostList(mappedAwsSetting);
        //     return HandleResponseFromStatusCode(statusCode, response);
        // }
        /// <summary>
        /// Get recommendation club list
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /v1/Recommendation/get-recommendation-club-list
        ///     {
        ///        
        ///        
        ///     }
        ///
        /// </remarks>
        //[LogRequest]
        //[LogResponse]
        // [HttpGet, Route("get-recommendation-club-list")]
        // public async Task<IActionResult> GetRecommendationClubList()
        // {
        //     AppSettingData appSettingData = new AppSettingData();
        //     var mappedAwsSetting = appSettingData.MapObject<AmazonSettingsCommon>();
        //     var (statusCode, response) = await _business.GetRecommendationClubList(mappedAwsSetting);
        //     return HandleResponseFromStatusCode(statusCode, response);
        // }
        /// <summary>
        /// Get home page recommendation club and host list
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /v1/Recommendation/get-homepage-recommendation-list
        ///     {
        ///        
        ///        
        ///     }
        ///
        /// </remarks>
        //[LogRequest, LogResponse]
        // [HttpGet, Route("get-homepage-recommendation-list")]
        // public async Task<IActionResult> GetHomepageRecommendationClubAndHostList()
        // {
        //     AppSettingData appSettingData = new AppSettingData();
        //     var mappedAwsSetting = appSettingData.MapObject<AmazonSettingsCommon>();
        //     var (statusCode, response) = await _business.GetHomepageRecommendationClubAndHostList(mappedAwsSetting);
        //     return HandleResponseFromStatusCode(statusCode, response);
        // }
        /// <summary>
        /// Get main page recommendation club and host list
        /// </summary>
        /// <param name="locationId"></param>
        /// <param name="positionId"></param>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /v1/Recommendation/get-mainpage-recommendation-list
        ///     {
        ///        "locationId": 1,
        ///        "positionId": "1",
        ///     }
        ///
        /// </remarks>
       // [LogRequest, LogResponse]
        // [HttpGet, Route("get-mainpage-recommendation-list")]
        // public async Task<IActionResult> GetMainpageRecommendationClubAndHostList(string? locationId, int? positionId)
        // {
        //     AppSettingData appSettingData = new AppSettingData();
        //     var mappedAwsSetting = appSettingData.MapObject<AmazonSettingsCommon>();
        //     var (statusCode, response) = await _business.GetMainpageRecommendationClubAndHostList(mappedAwsSetting, locationId, positionId);
        //     return HandleResponseFromStatusCode(statusCode, response);
        // }

        ///<summary>
        ///Get main page recommended club list
        ///</summary>
        ///<param name="locationId"></param>
        ///<param name="positionId"></param> 
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /v1/Recommendation/get-mainpage-recommended-club-list
        ///     {
        ///        "locationId": 1,
        ///        "positionId": "1",
        ///     }
        ///
        /// </remarks>
        //[LogRequest, LogResponse]
        [HttpGet, Route("get-mainpage-recommended-club-list")]
        public async Task<IActionResult> GetMainPageRecommendedClubList(string locationId, int positionId)
        {
            var (statusCode, response) = await _business.GetMainPageRecommendedClubList(locationId, positionId);
            return HandleResponseFromStatusCode(statusCode, response);
        }
        ///<summary>
        ///Get main page recommended host list
        ///</summary>
        ///<param name="locationId"></param>
        ///<param name="positionId"></param> 
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /v1/Recommendation/get-mainpage-recommended-host-list
        ///     {
        ///        "locationId": 1,
        ///        "positionId": "1",
        ///     }
        ///
        /// </remarks>
        //[LogRequest, LogResponse]
        [HttpGet, Route("get-mainpage-recommended-host-list")]
        public async Task<IActionResult> GetMainPageRecommendedHostList(string locationId, int positionId)
        {
            var (statusCode, response) = await _business.GetMainPageRecommendedHostList(locationId, positionId);
            return HandleResponseFromStatusCode(statusCode, response);
        }

        [HttpGet, Route("get-editors-recommendation-club-list")]
        public async Task<IActionResult> GetEditorsRecommendationClubList(string locationId, int? pageNo,int? pageSize)
        {
            var (statusCode, response) = await _business.GetEditorsRecommendationClubList(locationId, pageNo,pageSize);
            return HandleResponseFromStatusCode(statusCode, response);
        }

        [HttpGet, Route("get-editors-recommendation-club-detail-list")]
        public async Task<IActionResult> GetEditorsRecommendationClubDetailList(string? locationId, int? pageNo,
            int? pageSize)
        {
            var (statusCode,response) = await _business.GetEditorsRecommendationClubDetailList(locationId,pageNo,pageSize);
            return HandleResponseFromStatusCode(statusCode, response);
        }
    }
}
