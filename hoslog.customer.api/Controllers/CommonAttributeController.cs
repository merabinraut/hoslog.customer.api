using System.Net;
using Asp.Versioning;
using hoslog.customer.business.CommonAttribute;
using hoslog.customer.shared;
using Microsoft.AspNetCore.Mvc;

namespace hoslog.customer.api.Controllers
{

    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1.0")]
    public class CommonAttributeController : BaseApiController
    {
        private readonly ICommonAttributeBusiness _commonBusiness;

        public CommonAttributeController(ICommonAttributeBusiness commonBusiness)
        {
            _commonBusiness = commonBusiness;
        }
        /// <summary>
        /// Get Advertisement 
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        [HttpGet("get-common-attribute")]
        public async Task<IActionResult> GetAdvertisement(string flag)
        {
            var (statusCode, response) = await _commonBusiness.GetCommonAttributeList(flag);
            return HandleResponseFromStatusCode(statusCode, response);
        }

        [HttpGet("check-domain")]
        public async Task<IActionResult> CheckDomain()
        {
            var (statusCode, response) = (HttpStatusCode.OK, new CommonApiResponse
            {
                code = ResponseCodes.Code200_Success,
                message = "Success",
                data = null,
                errors = new List<Errors>()
            });
            return HandleResponseFromStatusCode(statusCode, response);
        }

        [HttpGet("clear-cache")]
        public async Task<IActionResult> ClearAllCache()
        {
            try
            {
                await _commonBusiness.ClearAllCacheAsync();
                var (statusCode, response) = (HttpStatusCode.OK, new CommonApiResponse
                {
                    code = ResponseCodes.Code200_Success,
                    message = "Cache cleared successfully.",
                    errors = new List<Errors>()
                });
                return HandleResponseFromStatusCode(statusCode, response);
            }
            catch (Exception ex)
            {
                var (statusCode, response) = (HttpStatusCode.BadRequest, new CommonApiResponse
                {
                    code = ResponseCodes.Code400_BadRequest,
                    message = "An error occurred while clearing the cache. Exception message:" + ex.Message,
                    errors = new List<Errors>()
                });
                return HandleResponseFromStatusCode(statusCode, response);
            }
        }

        [HttpPost("referral-clicks/{referCode}")]
        public async Task<IActionResult> UpdateReferralClickCount([FromRoute] string referCode)
        {
            var (statusCode, response) = await _commonBusiness.UpdateReferralClickCount(referCode);
            return HandleResponseFromStatusCode(statusCode, response);
        }
    }
}
