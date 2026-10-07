using Asp.Versioning;
using hoslog.customer.business.Profile;
using Microsoft.AspNetCore.Mvc;

namespace hoslog.customer.api.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1.0")]
    public class GalleryController : BaseApiController
    {
        private readonly IProfileBusiness _profileBusiness;
        public GalleryController(IProfileBusiness profileBusiness)
        {
            _profileBusiness = profileBusiness;
        }

        /// <summary>
        /// Get gallery images of host
        /// </summary>
        /// <param name="clubId">Encrypted club id</param>
        /// <param name="clubCode">Club code</param>
        /// <param name="pageNo">Page number</param>
        /// <returns></returns>
        [HttpGet, Route("get-gallery-image")]
        public async Task<IActionResult> GetGalleryImages(string clubId = "", string clubCode = "", int pageNo = 1)
        {
            var (statusCode, response) = await _profileBusiness.GetGalleryPhotoAsync(clubId, clubCode, pageNo);
            return HandleResponseFromStatusCode(statusCode, response);
        }
    }
}
