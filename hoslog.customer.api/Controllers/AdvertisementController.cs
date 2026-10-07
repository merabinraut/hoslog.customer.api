using Asp.Versioning;
using hoslog.customer.business.Advertisement;
using Microsoft.AspNetCore.Mvc;

namespace hoslog.customer.api.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1.0")]
    public class AdvertisementController : BaseApiController
    {
        private readonly IAdvertisementBusiness _adsBusiness;


        public AdvertisementController(IAdvertisementBusiness adsBusiness)
        {
            _adsBusiness = adsBusiness;
        }
        /// <summary>
        /// Get list of advertisement
        /// </summary>
        /// <returns></returns>
        [HttpGet("get-advertisement-list")]
        public async Task<IActionResult> GetAdvertisement()
        {
            var (statusCode, response) = await _adsBusiness.GetAdvertisementList();
            return HandleResponseFromStatusCode(statusCode, response);
        }
    }
}
