using Asp.Versioning;
using hoslog.customer.api.Filters;
using hoslog.customer.business.PointManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hoslog.customer.api.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1.0")]
    [Authorize]
    public class PointManagementController : BaseApiController
    {
        private readonly IPointManagementBusiness _buss;
        public PointManagementController(IPointManagementBusiness buss)
        {
            _buss = buss;
        }

        /// <summary>
        /// Manage Bookmark
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="fetch">Fetch</param>
        /// <param name="transactionMode">Transaction Mode (CR/DR)</param>
        /// <returns></returns>
        //[LogRequest]
        //[LogResponse]
        [HttpGet("get-user-point")]
        public async Task<IActionResult> GetUserPoint(int? row = 0, int? fetch = 10, string transactionMode = "")
        {
            var (statusCode, response) = await _buss.GetPoint(row, fetch, transactionMode);
            return HandleResponseFromStatusCode(statusCode, response);
        }
    }
}