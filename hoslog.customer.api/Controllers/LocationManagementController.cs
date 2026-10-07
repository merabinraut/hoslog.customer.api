using Asp.Versioning;
using hoslog.customer.business.LocationManagement;
using Microsoft.AspNetCore.Mvc;

namespace hoslog.customer.api.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1.0")]
  
    public class LocationManagementController : BaseApiController
    {
        private readonly ILocationManagementBusiness _locBusiness;

        public LocationManagementController(ILocationManagementBusiness locBusiness)
        {
            _locBusiness = locBusiness;
        }
        /// <summary>
        /// Get list of location
        /// </summary>
        /// <returns></returns>
        [HttpGet("get-location-list")]
        public async Task<IActionResult> GetLocation()
        {
            var (statusCode, response) = await _locBusiness.GetLocationList();
            return HandleResponseFromStatusCode(statusCode, response);
        }
    }
}
