using Asp.Versioning;
using hoslog.customer.business.Plans;
using Microsoft.AspNetCore.Mvc;

namespace hoslog.customer.api.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1.0")]
    public class PlanController : BaseApiController
    {
        private readonly IPlanBusiness _planBusiness;

        public PlanController(IPlanBusiness planBusiness)
        {
            _planBusiness = planBusiness;
        }

        [HttpGet("get-admin-plan-list")]
        public async Task<IActionResult> GetAdminPlan()
        {
            var (statusCode, response) = await _planBusiness.GetAdminPlanList();
            return HandleResponseFromStatusCode(statusCode, response);
        }

        [HttpGet("get-club-plan-list")]
        public async Task<IActionResult> GetClubPlan(string clubId = "")
        {
            var (statusCode, response) = await _planBusiness.GetClubPlanList(clubId);
            return HandleResponseFromStatusCode(statusCode, response);
        }

        [HttpGet("get-club-all-plan-list")]
        public async Task<IActionResult> GetClubAllPlanList(string clubId = "", string clubCode = "", int pageNo = 1)
        {
            var (statusCode, response) = await _planBusiness.GetClubAllPlanList(clubId, clubCode, pageNo);
            return HandleResponseFromStatusCode(statusCode, response);
        }
    }
}
