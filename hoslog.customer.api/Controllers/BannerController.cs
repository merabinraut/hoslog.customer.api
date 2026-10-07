using Asp.Versioning;
using hoslog.customer.business.Banner;
using Microsoft.AspNetCore.Mvc;

namespace hoslog.customer.api.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1.0")]
    public class BannerController : BaseApiController
    {
        private readonly IBannerBusiness _bannerBusiness;
        public BannerController(IBannerBusiness bannerBusiness)
        {
            _bannerBusiness = bannerBusiness;
        }

        /// <summary>
        ///  Get Banner List.
        /// </summary>
        /// <param></param>
        /// <returns>Return success/fail status</returns>
        /// <remarks>
        /// </remarks>
        /// <response code="200">Success</response>
        /// <response code="401">Failed/Unauthorized</response>        
        [HttpGet("get-banner-list")]
        public async Task<IActionResult> GetBanner()
        {
            var (statusCode, response) = await _bannerBusiness.GetBannerList();
            return HandleResponseFromStatusCode(statusCode, response);
        }
    }
}
