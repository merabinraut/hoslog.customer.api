using hoslog.customer.api.Library;
using hoslog.customer.business.Auth;
using hoslog.customer.business.User;
using hoslog.customer.common.Models.Auth;
using hoslog.customer.common.Service;
using hoslog.customer.common.Utilities;
using hoslog.customer.shared;
using hoslog.customer.shared.User;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace hoslog.customer.api.Filters
{
    public class BearerAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IAuthBusiness _authBusiness;
        private readonly IUserBusiness _userBusiness;
        private readonly StringCipher _stringCipher;
        private readonly ApiResponseMessageService _apiResponseMessageService;
        private const string WWW_AUTHENTICATE_HEADER = "WWW-Authenticate";
        private const string BEARER_REALM = "Bearer realm=\"Hoslog\", charset=\"UTF-8\"";
        public BearerAuthenticationHandler(
              IOptionsMonitor<AuthenticationSchemeOptions> options,
              ILoggerFactory logger,
              UrlEncoder encoder,
              ISystemClock clock,
              IAuthBusiness authBusiness,
              IUserBusiness userBusiness,
              StringCipher stringCipher,
              ApiResponseMessageService apiResponseMessageService)
              : base(options, logger, encoder, clock)
        {
            _authBusiness = authBusiness;
            _userBusiness = userBusiness;
            _stringCipher = stringCipher;
            _apiResponseMessageService = apiResponseMessageService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // var endpoint = Context.GetEndpoint();
            // if (endpoint?.Metadata?.GetMetadata<AllowAnonymousAttribute>() != null)
            //     return AuthenticateResult.NoResult();

            if (!Request.Headers.ContainsKey("Authorization"))
            {
                Response.Headers[WWW_AUTHENTICATE_HEADER] = BEARER_REALM;
                return AuthenticateResult.Fail("Missing Authorization Header");
            }

            if (!Request.Headers.ContainsKey("Authorization"))
            {
                Response.Headers[WWW_AUTHENTICATE_HEADER] = BEARER_REALM;
                return AuthenticateResult.Fail("Missing Authorization Header");
            }

            string authorizationHeader = Request.Headers["Authorization"];
            string token = GetBearerToken(authorizationHeader);

            if (string.IsNullOrEmpty(token))
            {
                await RespondUnauthorized(ResponseCodes.CE476_InvalidAuthorizationHeader);
                return AuthenticateResult.Fail("Invalid Authorization Header");
            }

            try
            {
                var claims = GetClaimsFromToken(token);
                if (claims == null)
                {
                    await RespondUnauthorized(ResponseCodes.CE478_InvalidToken);
                    return AuthenticateResult.Fail("Invalid token");
                }

                string username = claims.FirstOrDefault(c => c.Type == "username")?.Value ?? string.Empty;
                if (string.IsNullOrEmpty(username))
                    return await HandleInvalidSession(ResponseCodes.Code403_Forbidden);

                var (userStatusCode, user) = await _userBusiness.GetUserDetailViaUsername(username);
                var userDetail = user?.Adapt<CommonApiResponse>()?.data?.Adapt<UserDetailCommon>();

                if (userStatusCode != HttpStatusCode.OK || userDetail == null || userDetail?.Status?.ToUpper() != "A")
                    return await HandleInvalidSession(ResponseCodes.Code403_Forbidden);

                if (!await _authBusiness.ValidateTokenAsync(token, userDetail?.UserId, userDetail?.RoleType))
                {
                    await RespondUnauthorized(ResponseCodes.CE477_Unauthorized);
                    return AuthenticateResult.Fail("Unauthorized");
                }

                var userClaims = BuildUserClaims(userDetail);
                var ticket = BuildAuthenticationTicket(userClaims);

                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception occurred while processing the token.");
                Response.Headers[WWW_AUTHENTICATE_HEADER] = BEARER_REALM;
                return AuthenticateResult.Fail("An error occurred during authentication.");
            }
        }

        private string GetBearerToken(string authorizationHeader)
        {
            const string BEARER_PREFIX = "Bearer ";
            return authorizationHeader.StartsWith(BEARER_PREFIX, StringComparison.OrdinalIgnoreCase)
                ? authorizationHeader.Substring(BEARER_PREFIX.Length).Trim()
                : null;
        }

        private IEnumerable<Claim> GetClaimsFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            return jsonToken?.Claims;
        }

        private IEnumerable<Claim> BuildUserClaims(UserDetailCommon userDetail)
        {
            return new[]
            {
                new Claim(UserClaimsName.Username, _stringCipher.Encrypt(userDetail?.NickName) ?? ""),
                new Claim(UserClaimsName.UserId, _stringCipher.Encrypt(userDetail?.UserId) ?? ""),
                new Claim(UserClaimsName.AgentId, _stringCipher.Encrypt(userDetail?.AgentId) ?? ""),
                new Claim(UserClaimsName.Email, _stringCipher.Encrypt(userDetail?.EmailAddress) ?? ""),
                new Claim(UserClaimsName.MobileNumber, _stringCipher.Encrypt(userDetail?.MobileNumber) ?? ""),
                new Claim(UserClaimsName.RoleType, _stringCipher.Encrypt(userDetail?.RoleType) ?? "")
            };
        }

        private AuthenticationTicket BuildAuthenticationTicket(IEnumerable<Claim> userClaims)
        {
            var claimsIdentity = new ClaimsIdentity(userClaims, Scheme.Name);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            return new AuthenticationTicket(claimsPrincipal, Scheme.Name);
        }

        private async Task RespondUnauthorized(string message)
        {
            Response.Headers["WWW-Authenticate"] = "Bearer realm=\"Hoslog\", charset=\"UTF-8\"";
            Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            var response = new CommonApiResponse
            {
                data = null,
                code = ResponseCodes.Code401_Unauthorized,
                message = await _apiResponseMessageService.GetMessageByCodeAsync(message)
            };

            await Response.WriteAsJsonAsync(response);
        }

        private async Task RespondForbidden(string message)
        {
            Response.Headers["WWW-Authenticate"] = "Bearer realm=\"Hoslog\", charset=\"UTF-8\"";
            Response.StatusCode = (int)HttpStatusCode.Forbidden;

            var response = new CommonApiResponse
            {
                code = ResponseCodes.Code403_Forbidden,
                message = await _apiResponseMessageService.GetMessageByCodeAsync(message),
                data = null
            };

            await Response.WriteAsJsonAsync(response);
        }

        private async Task<AuthenticateResult> HandleInvalidSession(string responseCode)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/"
            };
            Response.Cookies.Append("_session_expired_", "1", cookieOptions);
            await RespondForbidden(responseCode);
            return AuthenticateResult.Fail("Forbidden");
        }
    }
}
