using System.Security.Cryptography;
using System.Text;
using hoslog.customer.business.Auth;
using hoslog.customer.common.Constants;
using hoslog.customer.common.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace hoslog.customer.api.Controllers.Auth;

[ApiController]
[Route("api/internal/customer-auth")]
[AllowAnonymous]
public sealed class InternalCustomerAuthController : BaseApiController
{
    private readonly IAuthBusiness _authBusiness;
    private readonly CognitoOptions _options;

    public InternalCustomerAuthController(
        IAuthBusiness authBusiness,
        IOptions<CognitoOptions> options
    )
    {
        _authBusiness = authBusiness;
        _options = options.Value;
    }

    [HttpPost("otp/verify")]
    public async Task<IActionResult> VerifyOtpFromCognitoLambda
    (
        [FromBody] InternalOtpVerifyRequest request,
         CancellationToken cancellationToken
    )
    {
        var requestSecret = Request.Headers["X-Internal-Secret"].FirstOrDefault();

        if (!IsSecretValid(requestSecret, _options.InternalOtpSecret))
        {
            return Unauthorized(new
            {
                isValid = false
            });
        }
        if (!string.Equals(request.purpose, OtpPurposeConstants.Login, StringComparison.OrdinalIgnoreCase))
            return BadRequest(new
            {
                isValid = false
            });

        var isValid = await _authBusiness.VerifyOtpWithOutTransactionAsync(
            request.username,
            OtpPurposeConstants.Login,
            request.otpCode,
            request.submittedOtpHash
        );
        return Ok(new
        {
            isValid
        });
    }

    private static bool IsSecretValid(
       string? requestSecret,
       string configuredSecret
   )
    {
        if (
            string.IsNullOrWhiteSpace(requestSecret) ||
            string.IsNullOrWhiteSpace(configuredSecret)
        )
            return false;

        var requestBytes = Encoding.UTF8.GetBytes(requestSecret);
        var configuredBytes = Encoding.UTF8.GetBytes(configuredSecret);

        if (requestBytes.Length != configuredBytes.Length)
            return false;

        return CryptographicOperations.FixedTimeEquals(requestBytes, configuredBytes);
    }

    public sealed class InternalOtpVerifyRequest
    {
        public string username { get; set; } = string.Empty;
        public string purpose { get; set; } = string.Empty;
        public string otpCode { get; set; } = string.Empty;
        public string submittedOtpHash { get; set; } = string.Empty;
    }
}