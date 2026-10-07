using Asp.Versioning;
using hoslog.customer.business.ReferralManagement;
using Microsoft.AspNetCore.Mvc;

namespace hoslog.customer.api.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1.0")]
    public class ReferralPointController : BaseApiController
    {
        private readonly IReferralPointBusiness _referralPointInfoBusiness;
        public ReferralPointController(IReferralPointBusiness referralPointInfoBusiness)
        {
            _referralPointInfoBusiness = referralPointInfoBusiness;
        }
        [HttpGet("get-referral-point-info")]
        public async Task<IActionResult> GetReferralPointInfo()
        {
            var (statusCode, result) = await _referralPointInfoBusiness.GetReferralPointInfo();
            return HandleResponseFromStatusCode(statusCode, result);
        }

        [HttpGet("get-total-referral-points")]
        public async Task<IActionResult> GetTotalReferralPoint()
        {
            var (statusCode, result) = await _referralPointInfoBusiness.GetTotalReferralPoint();
            return HandleResponseFromStatusCode(statusCode, result);
        }

        [HttpGet("get-referral-info-status")]
        public async Task<IActionResult> GetReferralInfoStatus()
        {
            var (statusCode, result) = await _referralPointInfoBusiness.GetReferralInfoStatus();
            return HandleResponseFromStatusCode(statusCode, result);
        }
    }
}
