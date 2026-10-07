using Asp.Versioning;
using hoslog.customer.api.Filters;
using hoslog.customer.api.Library;
using hoslog.customer.business.Review;
using hoslog.customer.common.Models;
using hoslog.customer.common.Models.Auth;
using hoslog.customer.common.Models.ReviewModel;
using hoslog.customer.shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hoslog.customer.api.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1.0")]
    public class ReviewController : BaseApiController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IReviewBusiness _revBusiness;

        public ReviewController(IReviewBusiness revBusiness, IHttpContextAccessor httpContextAccessor)
        {
            _revBusiness = revBusiness;
            _httpContextAccessor = httpContextAccessor;
        }
        /// <summary>
        /// Get user reviewed club
        /// </summary>
        /// <param name="pageNo"></param>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /v1/Review/get-user-review-list
        ///     {
        ///        "pageNo":0
        ///        
        ///     }
        ///
        /// </remarks>
        //[LogRequest]
        //[LogResponse]
        [HttpGet("get-user-review-list")]
        [Authorize]
        public async Task<IActionResult> GetReviewList(int pageNo, string type = "all")
        {
            AppSettingData appSettingData = new AppSettingData();
            var userId = _httpContextAccessor.HttpContext?.User?.Claims.First(x => x.Type == UserClaimsName.UserId).Value;
            var awsSettingModel = appSettingData.GetAmazonSetting;
            var mappedAwsSetting = awsSettingModel.MapObject<AmazonSettingsCommon>();
            PaginationRequestModel objPaginationRequestModel = new PaginationRequestModel();
            objPaginationRequestModel.pageNo = pageNo;
            var (statusCode, response) = await _revBusiness.GetUserReviewList(objPaginationRequestModel, userId, mappedAwsSetting, type);
            return HandleResponseFromStatusCode(statusCode, response);
        }
        /// <summary>
        /// Get user to review the reserved club details
        /// </summary>
        /// <param name="reservationId"></param>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /v1/Review/get-review-reservation-detail
        ///     {
        ///        "reservationId" : "V+iaDwcDLqHEowK+IHuVxA=="
        ///        
        ///     }
        ///
        /// </remarks>
        //[LogRequest, LogResponse]
        [HttpGet("get-review-reservation-detail")]
        [Authorize]
        public async Task<IActionResult> ReviewStep1(string reservationId)
        {
            AppSettingData appSettingData = new AppSettingData();
            string userId = _httpContextAccessor.HttpContext?.User?.Claims.First(x => x.Type == UserClaimsName.UserId).Value;
            var awsSettingModel = appSettingData.GetAmazonSetting;
            var mappedAwsSetting = awsSettingModel.MapObject<AmazonSettingsCommon>();
            var (statusCode, response) = await _revBusiness.GetReservationDetails(reservationId, userId, mappedAwsSetting);
            return HandleResponseFromStatusCode(statusCode, response);
        }
        /// <summary>
        /// Get club host list for review the reservation
        /// </summary>
        /// <param name="clubId"></param>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /v1/Review/get-review-hostlist
        ///     {
        ///        "clubId" : "ChG25FTFb3+6Xdq2kBZGoA=="
        ///        
        ///     }
        ///
        /// </remarks>
        //[LogRequest, LogResponse]
        [HttpGet("get-review-hostlist")]
        [Authorize]
        public async Task<IActionResult> ReviewStep2(string clubId)
        {
            AppSettingData appSettingData = new AppSettingData();
            var awsSettingModel = appSettingData.GetAmazonSetting;
            var mappedAwsSetting = awsSettingModel.MapObject<AmazonSettingsCommon>();
            var (statusCode, response) = await _revBusiness.GetReviewHostListViaClubId(mappedAwsSetting, clubId);
            return HandleResponseFromStatusCode(statusCode, response);
        }/// <summary>
         /// Get review remarks list
         /// </summary>
         /// <param name="clubId"></param>
         /// <remarks>
         /// Sample request:
         ///
         ///     GET /v1/Review/get-review-remark-list
         ///     {
         ///        "clubId" : "ChG25FTFb3+6Xdq2kBZGoA=="
         ///        
         ///     }
         ///
         /// </remarks>
        //[LogRequest, LogResponse]
        [HttpGet("get-review-remark-list")]
        [Authorize]
        public async Task<IActionResult> ReviewStep3(string clubId)
        {
            AppSettingData appSettingData = new AppSettingData();
            var awsSettingModel = appSettingData.GetAmazonSetting;
            var mappedAwsSetting = awsSettingModel.MapObject<AmazonSettingsCommon>();
            var (statusCode, response) = await _revBusiness.GetReviewRemarkList(clubId, mappedAwsSetting);
            return HandleResponseFromStatusCode(statusCode, response);
        }
        /// <summary>
        /// Get review question and answer list
        /// </summary>
        /// <param name="clubId"></param>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /v1/Review/get-review-qna-list
        ///     {
        ///        "clubId" : "ChG25FTFb3+6Xdq2kBZGoA=="
        ///        
        ///     }
        ///
        /// </remarks>
        //[LogRequest, LogResponse]
        [HttpGet("get-review-qna-list")]
        [Authorize]
        public async Task<IActionResult> ReviewStep4(string clubId)
        {
            AppSettingData appSettingData = new AppSettingData();
            var awsSettingModel = appSettingData.GetAmazonSetting;
            var mappedAwsSetting = awsSettingModel.MapObject<AmazonSettingsCommon>();
            var (statusCode, response) = await _revBusiness.GetReviewQNAList(clubId, mappedAwsSetting);
            return HandleResponseFromStatusCode(statusCode, response);
        }
        /// <summary>
        /// Get review summary detail
        /// </summary>
        /// <param name="clubId"></param>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /v1/Review/get-review-detail
        ///     {
        ///        "clubId" : "ChG25FTFb3+6Xdq2kBZGoA=="
        ///        
        ///     }
        ///
        /// </remarks>
        //[LogRequest, LogResponse]
        [HttpGet("get-review-detail")]
        [Authorize]
        public async Task<IActionResult> GetReviewDetail(string clubId)
        {
            AppSettingData appSettingData = new AppSettingData();
            var awsSettingModel = appSettingData.GetAmazonSetting;
            var mappedAwsSetting = awsSettingModel.MapObject<AmazonSettingsCommon>();
            var (statusCode, response) = await _revBusiness.GetReviewDetail(clubId, mappedAwsSetting);
            return HandleResponseFromStatusCode(statusCode, response);
        }
        /// <summary>
        /// Manage the review details which get logged into user profile
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [LogRequest, LogResponse]
        [HttpPost("manage-review")]
        [Authorize]
        public async Task<IActionResult> ManageReview([FromBody] ManageReviewRequestModel requestModel)
        {
            requestModel.userId = _httpContextAccessor.HttpContext?.User?.Claims.First(x => x.Type == UserClaimsName.UserId).Value;
            var (statusCode, response) = await _revBusiness.ManageReview(requestModel);
            return HandleResponseFromStatusCode(statusCode, response);
        }
    }
}
