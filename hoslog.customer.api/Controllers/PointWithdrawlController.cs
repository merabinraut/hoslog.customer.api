using Asp.Versioning;
using hoslog.customer.business.PointWithdrawl;
using hoslog.customer.common.Models.PointWithdrawl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hoslog.customer.api.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1.0")]
    [Authorize]
    public class PointWithdrawlController : BaseApiController
    {
        private readonly IPointWithdrawlBusiness _pointWithdrawlBusiness;
        public PointWithdrawlController(IPointWithdrawlBusiness pointWithdrawlBusiness) 
        {
            _pointWithdrawlBusiness = pointWithdrawlBusiness;           
            
        }

        [HttpPost("point-redeem-giftcard")]
        public async Task<IActionResult> RedeemPointGiftCard(RedeemPointGiftCardModel model)
        {
            var (statusCode, result) = await _pointWithdrawlBusiness.RedeemPointGiftCard(model);
            return HandleResponseFromStatusCode(statusCode, result);
        }
    }
}
