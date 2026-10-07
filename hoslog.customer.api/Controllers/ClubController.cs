using Asp.Versioning;
using hoslog.customer.api.Filters;
using hoslog.customer.api.Library;
using hoslog.customer.business.ClubManagement;
using hoslog.customer.common.Models.ClubManagement;
using hoslog.customer.shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hoslog.customer.api.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1.0")]
    public class ClubController : BaseApiController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IClubManagementBusiness _clubBusiness;

        public ClubController(IClubManagementBusiness clubBusiness, IHttpContextAccessor httpContextAccessor)
        {
            _clubBusiness = clubBusiness;
            _httpContextAccessor = httpContextAccessor;
        }
        /// <summary>
        /// Get club detail list.
        /// </summary>
        /// <param name="pageNo">Page number for pagination.</param>
        /// <param name="search">Search by club name.</param>
        /// <returns></returns>
        //[LogRequest]
        //[LogResponse]
        [HttpGet("get-club-detail-list")]
        public async Task<IActionResult> GetClubDetailList(int pageNo, string? search)
        {
            ClubDetailModel model = new ClubDetailModel();
            model.pageNo = pageNo;
            model.search = search;
            var (statusCode, response) = await _clubBusiness.GetClubDetailList(model);
            return HandleResponseFromStatusCode(statusCode, response);
        }
        /// <summary>
        /// Get specific club basic information.
        /// </summary>
        /// <param name="clubId">Encrypted Club ID</param>
        /// <returns></returns>
        [AllowAnonymous]
        [ServiceFilter(typeof(SubDomainNameValidatorFilterV2))]
        //[LogRequest, LogResponse]
        [HttpGet, Route("get-club-basic-information")]
        public async Task<IActionResult> GetClubBasicInfoDetail(string clubId = "", string clubCode = "")
        {
            var (statusCode, response) = await _clubBusiness.GetClubBasicInfoDetail(clubId, clubCode);
            return HandleResponseFromStatusCode(statusCode, response);
        }
        /// <summary>
        /// Get specific club schedule.
        /// </summary>
        /// <param name="clubId">Encrypted Club ID</param>
        /// <param name="duration">The duration value must be between 1 and 2 weeks. Even if the duration value exceeds 2, only a maximum of 2 weeks of details will be fetched.</param>
        /// <returns></returns>
        [AllowAnonymous]
        //[LogRequest, LogResponse]
        //[HttpGet, Route("get-club-weekly-schedule/{clubId}/{duration}")]
        [HttpGet, Route("get-club-weekly-schedule")]
        public async Task<IActionResult> GetClubWeeklySchedule(int duration, string? clubId = null, string? clubCode = null)
        {
            var (statusCode, response) = await _clubBusiness.GetClubWeeklySchedule(clubId, clubCode, duration);
            return HandleResponseFromStatusCode(statusCode, response);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clubId"></param>
        /// <param name="clubCode"></param>
        /// <param name="pageNo"></param>
        /// <returns></returns>
        [AllowAnonymous]
        //[LogRequest, LogResponse]
        [HttpGet, Route("get-club-review-and-rating")]
        public async Task<IActionResult> GetClubReviewAndRatings(string clubId = "", string clubCode = "", int pageNo = 1)
        {
            AppSettingData appSettingData = new AppSettingData();
            var mappedAwsSetting = appSettingData.MapObject<AmazonSettingsCommon>();
            var (statusCode, response) = await _clubBusiness.GetClubReviewAndRatings(mappedAwsSetting, clubId, clubCode, pageNo);
            return HandleResponseFromStatusCode(statusCode, response);
        }

        [AllowAnonymous]
        [HttpGet, Route("get-shop-you-can-vist-now")]
        public async Task<IActionResult> ShopsYouCanVisitNow(string locationId, int pageNo = 1)
        {
            var (statusCode, response) = await _clubBusiness.ShopsYouCanVisitNow(locationId, pageNo);
            return HandleResponseFromStatusCode(statusCode, response);
        }
    }
}
