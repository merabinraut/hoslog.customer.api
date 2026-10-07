using Asp.Versioning;
using hoslog.customer.api.Filters;
using hoslog.customer.api.Library;
using hoslog.customer.business.Notification;
using hoslog.customer.common.Models;
using hoslog.customer.shared;
using hoslog.customer.shared.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hoslog.customer.api.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1.0")]
    [Authorize]
    public class NotificationController : BaseApiController
    {
        private readonly INotificationBusiness _noticeBusiness;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public NotificationController(INotificationBusiness notificationBusiness, IHttpContextAccessor httpContextAccessor)
        {
            _noticeBusiness = notificationBusiness;
            _httpContextAccessor = httpContextAccessor;
        }
        /// <summary>
        /// Get notification list. 
        /// </summary>
        /// <param name="pageNo">Page number for pagination.</param>
        /// <returns></returns>
        //[LogRequest]
        //[LogResponse]
        [Authorize]
        [HttpGet, Route("get-notification-list")]
        public async Task<IActionResult> GetNotificationList(int pageNo)
        {
            AppSettingData appSettingData = new AppSettingData();
            var awsSettingModel = appSettingData.GetAmazonSetting;
            var mappedAwsSetting = awsSettingModel.MapObject<AmazonSettingsCommon>();
            var (statusCode, response) = await _noticeBusiness.GetNotificationList(pageNo, mappedAwsSetting);
            return HandleResponseFromStatusCode(statusCode, response);
        }
        /// <summary>
        ///Mark specific notification as read.
        /// </summary>
        /// <param name="mappedRequest"></param>
        /// <returns>Return success/fail status</returns>
        /// <remarks>
        /// **Sample request body:**
        ///
        ///  
        ///        {
        ///                "notificationId": "Encrypted notification id is here for mark as read"
        ///        }
        ///  
        ///  
        /// </remarks>
        /// <response code="200">Success</response>
        /// <response code="401">Failed/Unauthorized</response>


        [LogRequest]
        [LogResponse]
        [Authorize]
        [HttpPost, Route("mark-as-read")]
        public async Task<IActionResult> MarkAsReadNotification(NotificationMarkAsReadRequest request)
        {
            var requestMapped = request.MapObject<NotificationMarkAsReadRequestCommon>();
            var (statusCode, response) = await _noticeBusiness.MarkAsReadNotification(requestMapped);
            return HandleResponseFromStatusCode(statusCode, response);
        }
        /// <summary>
        ///Notificaiton mark all as read.
        /// </summary>
        /// <param name="mappedRequest"></param>
        /// <returns>Return success/fail status</returns>
        /// <remarks>
        /// **Sample request body:**
        ///
        ///  
        ///        {
        ///              
        ///        }
        ///  
        ///  
        /// </remarks>
        /// <response code="200">Success</response>
        /// <response code="401">Failed/Unauthorized</response>
        [LogRequest]
        [LogResponse]
        [Authorize]
        [HttpPost, Route("mark-all-as-read")]
        public async Task<IActionResult> MarkAllAsReadNotification()
        {
            var (statusCode, response) = await _noticeBusiness.MarkAllAsReadNotification();
            return HandleResponseFromStatusCode(statusCode, response);
        }

        //[LogRequest]
       // [LogResponse]
        [Authorize]
        [HttpGet, Route("check-unread-notification")]
        public async Task<IActionResult> CheckUnreadNotification()
        {

            var (statusCode, response) = await _noticeBusiness.CheckUnreadNotification();
            return HandleResponseFromStatusCode(statusCode, response);
        }

    }
}
