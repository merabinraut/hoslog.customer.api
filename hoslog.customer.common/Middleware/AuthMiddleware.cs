using hoslog.customer.common.Utilities;
using hoslog.customer.shared.User;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace hoslog.customer.common.Middleware
{
    public class AuthMiddleware
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly StringCipher _stringCipher;
        private readonly SigningCredentials _signingCredentials;
        private readonly TokenValidationParameters _tokenValidationParameters;
        public AuthMiddleware(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, StringCipher stringCipher)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _stringCipher = stringCipher;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            _signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = _configuration["JWT:Issuer"],
                ValidAudience = _configuration["JWT:Audience"],
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKey = securityKey,
                ClockSkew = TimeSpan.Zero
            };
        }

        public (string token, DateTime expiry) GenerateAccessToken(UserDetailCommon user, string traceId)
        {
            var claims = new[]
            {
            new Claim("LoginId", _stringCipher.Encrypt(user.LoginId)),
            new Claim("AgentId", _stringCipher.Encrypt(user.AgentId)),
            new Claim("UserId", _stringCipher.Encrypt(user.UserId)),
            new Claim("TraceId", _stringCipher.Encrypt(traceId))
            };

            int accessTokenValidityMinutes = int.TryParse(_configuration["JWT:AccessTokenValidityInMinutes"], out var minutes) ? minutes : 2;
            return GenerateToken(claims, accessTokenValidityMinutes);
        }

        public (string token, DateTime expiry) GenerateRefreshToken(string loginId, string userId, string traceId)
        {
            var claims = new[]
            {
                new Claim("LoginId", _stringCipher.Encrypt(loginId)),
                new Claim("UserId", _stringCipher.Encrypt(userId)),
                new Claim("TraceId", _stringCipher.Encrypt(traceId))
            };

            int refreshTokenValidityMinutes = int.TryParse(_configuration["JWT:RefreshTokenValidityMinutes"], out var minutes) ? minutes : 2;
            return GenerateToken(claims, refreshTokenValidityMinutes);
        }

        private (string token, DateTime expiry) GenerateToken(IEnumerable<Claim> claims, int expiryMinutes)
        {
            var tokenExpiry = DateTime.UtcNow.AddMinutes(expiryMinutes);
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: tokenExpiry,
                signingCredentials: _signingCredentials
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), tokenExpiry);
        }

        public (bool isValid, ClaimsPrincipal principal) GetPrincipalFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out SecurityToken securityToken);
                var jwtSecurityToken = securityToken as JwtSecurityToken;

                bool isValid = jwtSecurityToken != null &&
                           jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
                return (isValid, isValid ? principal : null);
            }
            catch
            {
                return (false, null);
            }
        }

    }
}
