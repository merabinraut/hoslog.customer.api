
using Asp.Versioning;
using hoslog.customer.api.Filters;
using hoslog.customer.business.Auth;
using hoslog.customer.business.Conversion;
using hoslog.customer.common.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hoslog.customer.api.Controllers
{
    [Route("api/v{version:apiVersion}")]
    [ApiVersion("1.0")]
    [AllowAnonymous]
    public class AuthController : BaseApiController
    {
        private readonly IConversionBusiness _business;
        private readonly IAuthBusiness _authBusiness;
        public AuthController(IAuthBusiness authBusiness, IConversionBusiness business)
        {
            _business = business;
            _authBusiness = authBusiness;
        }

        [HttpPost("login")]
        [LogRequest]
        [LogResponse]
        public async Task<IActionResult> Login(LoginRequestModel request)
        {
            var session = await _business.GetRequiredAsync(HttpContext);
            var (responseCode, response) = await _authBusiness.Login(request,session);
            return HandleResponseFromStatusCode(responseCode, response);
        }

        [HttpPost("refresh-token")]
        [LogRequest]
        [LogResponse]
        public async Task<IActionResult> Refresh(RefreshTokenRequestModel request)
        {
            var (responseCode, response) = await _authBusiness.Refresh(request);
            return HandleResponseFromStatusCode(responseCode, response);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var (responseCode, response) = await _authBusiness.LogoutAsync();
            return HandleResponseFromStatusCode(responseCode, response);
        }
    }
}
