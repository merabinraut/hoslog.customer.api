using Asp.Versioning;
using hoslog.customer.api.Filters;
using hoslog.customer.api.Library;
using hoslog.customer.business.Reservation;
using hoslog.customer.common.Models;
using hoslog.customer.common.Models.Auth;
using hoslog.customer.common.Models.Reservation;
using hoslog.customer.shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hoslog.customer.api.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1.0")]
    public class ReservationHistoryController : BaseApiController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IReservationHistoryBusiness _resBusiness;
        public ReservationHistoryController(IReservationHistoryBusiness resBusiness, IHttpContextAccessor httpContextAccessor)
        {
            _resBusiness = resBusiness;
            _httpContextAccessor = httpContextAccessor;
        }

        //[LogRequest]
        //[LogResponse]
        [HttpGet("get-reservation-list")]
        [Authorize]
        public async Task<IActionResult> GetReservationList(int pageNo, string reservationStatus = "")
        {
            AppSettingData appSettingData = new AppSettingData();
            var awsSettingModel = appSettingData.GetAmazonSetting;
            var mappedAwsSetting = awsSettingModel.MapObject<AmazonSettingsCommon>();
            var userId = _httpContextAccessor.HttpContext?.User?.Claims.First(x => x.Type == UserClaimsName.UserId).Value;
            PaginationRequestModel objPaginationRequestModel = new PaginationRequestModel();
            objPaginationRequestModel.pageNo = pageNo;
            var (statusCode, response) = await _resBusiness.GetReservationList(objPaginationRequestModel, mappedAwsSetting, userId, reservationStatus);
            return HandleResponseFromStatusCode(statusCode, response);
        }
        //[LogRequest]
       // [LogResponse]
        [HttpGet("get-reservation-detail")]
        [Authorize]
        public async Task<IActionResult> GetReservationDetail(string reservationId)
        {
            AppSettingData appSettingData = new AppSettingData();
            var awsSettingModel = appSettingData.GetAmazonSetting;
            var mappedAwsSetting = awsSettingModel.MapObject<AmazonSettingsCommon>();
            var userId = _httpContextAccessor.HttpContext?.User?.Claims.First(x => x.Type == UserClaimsName.UserId).Value;
            ReservationModel objReservationModel = new ReservationModel();
            objReservationModel.reservationId = reservationId;
            objReservationModel.actionPlatform = _httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("ActionPlatform", out var headerValue) == true ? headerValue.FirstOrDefault() : "web";
            var (statusCode, response) = await _resBusiness.GetReservationDetail(mappedAwsSetting, userId, objReservationModel);
            return HandleResponseFromStatusCode(statusCode, response);
        }
        [LogRequest]
        [LogResponse]
        [HttpPost("cancel-reservation")]
        [Authorize]
        public async Task<IActionResult> CancelReservation(ReservationCancelModel objReservationCancelModel)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.Claims.First(x => x.Type == UserClaimsName.UserId).Value;
            objReservationCancelModel.actionPlatform = _httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("ActionPlatform", out var headerValue) == true ? headerValue.FirstOrDefault() : "web";
            var (statusCode, response) = await _resBusiness.CancelReservation(objReservationCancelModel, userId);
            return HandleResponseFromStatusCode(statusCode, response);
        }
        [LogRequest]
        [LogResponse]
        [HttpPost("delete-reservation")]
        [Authorize]
        public async Task<IActionResult> DeleteReservation(ReservationCancelModel objReservationCancelModel)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.Claims.First(x => x.Type == UserClaimsName.UserId).Value;
            objReservationCancelModel.actionPlatform = _httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("ActionPlatform", out var headerValue) == true ? headerValue.FirstOrDefault() : "web";
            var (statusCode, response) = await _resBusiness.DeleteReservation(objReservationCancelModel, userId);
            return HandleResponseFromStatusCode(statusCode, response);
        }
        //[LogRequest]
        //[LogResponse]
        [HttpGet("get-reservation-time-detail")]
        [Authorize]
        public async Task<IActionResult> GetReservationTimeDetail(string? reservationId)
        {
            ReservationModel objReservationModel = new ReservationModel();
            var userId = _httpContextAccessor.HttpContext?.User?.Claims.First(x => x.Type == UserClaimsName.UserId).Value;
            objReservationModel.actionPlatform = _httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("ActionPlatform", out var headerValue) == true ? headerValue.FirstOrDefault() : "web";
            objReservationModel.reservationId = reservationId;
            var (statusCode, response) = await _resBusiness.GetReservationClubTime(userId, objReservationModel);
            return HandleResponseFromStatusCode(statusCode, response);
        }
        [LogRequest]
        [LogResponse]
        [HttpPost("update-reservation-time")]
        [Authorize]
        public async Task<IActionResult> UpdateReservationTime(ReservationTimelModel objReservationTimelModel)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.Claims.First(x => x.Type == UserClaimsName.UserId).Value;
            objReservationTimelModel.actionPlatform = _httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("ActionPlatform", out var headerValue) == true ? headerValue.FirstOrDefault() : "web";
            var (statusCode, response) = await _resBusiness.UpdateReservationTime(objReservationTimelModel, userId);
            return HandleResponseFromStatusCode(statusCode, response);
        }
    }
}
