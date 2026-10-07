using Asp.Versioning;
using hoslog.customer.business.Host;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hoslog.customer.api.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1.0")]
    [AllowAnonymous]
    public class HostController : BaseApiController
    {
        private readonly IHostBusiness _hostBusiness;
        public HostController(IHostBusiness hostBusiness)
        {
            _hostBusiness = hostBusiness;
        }

        /// <summary>
        /// Get host list
        /// </summary>
        /// <param name="clubId">Encrypted ID of club</param>
        /// <returns></returns>
        [HttpGet, Route("get-host-list")]
        public async Task<IActionResult> GetHostList(string? clubId = null, string? clubCode = null)
        {
            var (statusCode, response) = await _hostBusiness.GetHostListAsync(clubId, clubCode);
            return HandleResponseFromStatusCode(statusCode, response);
        }

        /// <summary>
        ///Get the club schedule, indicating which times are available.
        /// </summary>
        /// <param name="clubId">Encrypted club id</param>
        /// <param name="date">Search for a specific date to indicate which times are available on that date.</param>
        /// <returns></returns>
        [HttpGet, Route("get-club-schedule")]
        public async Task<IActionResult> GetScheduleList(long? date, string? clubId = null, string? clubCode = null)
        {
            var (statusCode, response) = await _hostBusiness.GetScheduleListAsync(clubId, clubCode, date);
            return HandleResponseFromStatusCode(statusCode, response);
        }

        /// <summary>
        /// Get host details of specific host.
        /// </summary>
        /// <param name="hostId">Encrypted host ID</param>
        /// <returns></returns>
        [HttpGet, Route("get-host-details")]
        public async Task<IActionResult> GetHostDetails(string? hostId = null, string? hostCode = null)
        {
            var (statusCode, response) = await _hostBusiness.GetHostDetailsAsync(hostId, hostCode);
            return HandleResponseFromStatusCode(statusCode, response);
        }

        /// <summary>
        /// Get host details of specific host.
        /// </summary>
        /// <param name="hostId">Encrypted host ID</param>
        /// <param name="hostCode">Host code</param>
        /// <returns></returns>
        [HttpGet, Route("get-host-details-v2")]
        public async Task<IActionResult> GetHostDetailsV2(string? hostId = null, long? hostCode = null)
        {
            var (statusCode, response) = await _hostBusiness.GetHostDetailsV2Async(hostId, hostCode ?? 0);
            return HandleResponseFromStatusCode(statusCode, response);
        }

        /// <summary>
        /// Get host list
        /// </summary>
        /// <param name="clubId">Encrypted ID of club</param>
        /// <param name="clubCode">Club code</param>
        /// <param name="pageNo">Page number</param>
        /// <returns></returns>
        [HttpGet, Route("get-hosts-with-rank-ordering")]
        public async Task<IActionResult> GetHostsWithRankOrderingAsync(string clubId = null, string clubCode = null, int pageNo = 1, int pageSize = 12)
        {
            var (statusCode, response) = await _hostBusiness.GetHostsWithRankOrderingAsync(clubId, clubCode, pageNo, pageSize);
            return HandleResponseFromStatusCode(statusCode, response);
        }
    }
}
