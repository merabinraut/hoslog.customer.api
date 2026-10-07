using Asp.Versioning;
using hoslog.customer.api.Filters;
using hoslog.customer.api.Library;
using hoslog.customer.business.Bookmark;
using hoslog.customer.shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hoslog.customer.api.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1.0")]
    [Authorize]
    public class BookmarkController : BaseApiController
    {
        private readonly IBookmarkBusiness _business;
        public BookmarkController(IBookmarkBusiness business)
        {
            _business = business;
        }

        /// <summary>
        /// Manage Bookmark
        /// </summary>
        /// <param name="agentId">Encrypted agent id.</param>
        /// <param name="hostId">Encrypted host id.</param>
        /// <param name="type">Club or Host for the type of bookmark .</param>
        /// <returns></returns>
        [LogRequest]
        [LogResponse]
        [HttpPost, Route("manage-bookmark")]
        public async Task<IActionResult> ManageBookmark(string agentId = "", string hostId = "", string type = "")
        {
            var (statusCode, response) = await _business.ManageBookmark(agentId, hostId, type);
            return HandleResponseFromStatusCode(statusCode, response);
        }
        /// <summary>
        /// Get bookmark list and club list  
        /// </summary>
        /// <returns></returns>
        //[LogRequest]
        //[LogResponse]
        [HttpGet, Route("get-bookmark-list")]
        public async Task<IActionResult> GetBookmarkHostAndClubList()
        {
            AppSettingData appSettingData = new AppSettingData();
            var mappedAwsSetting = appSettingData.MapObject<AmazonSettingsCommon>();
            var (statusCode, response) = await _business.GetBookmarkHostAndClubList(mappedAwsSetting);
            return HandleResponseFromStatusCode(statusCode, response);
        }
    }
}
