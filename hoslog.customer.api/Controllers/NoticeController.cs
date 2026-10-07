using Asp.Versioning;
using hoslog.customer.business.Notice;
using Microsoft.AspNetCore.Mvc;

namespace hoslog.customer.api.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1.0")]
    public class NoticeController : BaseApiController
    {
        private readonly INoticeBusiness _noticeBusiness;
        public NoticeController(INoticeBusiness noticeBusiness)
        {
            _noticeBusiness = noticeBusiness;
        }
        /// <summary>
        /// Get club  notice. 
        /// </summary>
        /// <param name="pageNo">Page number for pagination</param>
        /// <param name="clubId">Encrypted Club Id</param>
        /// <returns></returns>
        [HttpGet, Route("get-club-notice")]
        public async Task<IActionResult> GetclubNotice(int pageNo, string clubId = "", string clubCode = "")
        {
            var (statusCode, response) = await _noticeBusiness.GetClubNoticeAsync(pageNo, clubId, clubCode);
            return HandleResponseFromStatusCode(statusCode, response);
        }
    }
}
