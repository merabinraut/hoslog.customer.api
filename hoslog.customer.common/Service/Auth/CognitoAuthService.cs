using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using hoslog.customer.common.Models.Auth;
using hoslog.customer.common.Utilities;
using hoslog.customer.shared;
using Microsoft.Extensions.Options;

namespace hoslog.customer.common.Service;

public sealed class CognitoAuthService : ICognitoAuthService
{
    private readonly IAmazonCognitoIdentityProvider _cognito;
    private readonly CognitoOptions _options;
    public CognitoAuthService(
        IAmazonCognitoIdentityProvider cognito,
        IOptions<CognitoOptions> options
    )
    {
        _cognito = cognito;
        _options = options.Value;
    }

    public async Task CreateConfirmedCustomerUserAsync(
        string username,
        string fullName,
        string password,
        string email,
        string phoneNumber,
        CancellationToken cancellationToken = default
    )
    {
        username = OtpHashHelper.NormalizeUsername(username);

        var attributes = new List<AttributeType>();

        if (!string.IsNullOrEmpty(fullName))
        {
            attributes.Add(new AttributeType
            {
                Name = "name",
                Value = fullName ?? string.Empty
            });
        }

        if (!string.IsNullOrEmpty(email))
        {
            attributes.Add(new AttributeType
            {
                Name = "email",
                Value = email
            });

            attributes.Add(new AttributeType
            {
                Name = "email_verified",
                Value = "true"
            });
        }

        if (!string.IsNullOrEmpty(phoneNumber))
        {
            attributes.Add(new AttributeType
            {
                Name = "phone_number",
                Value = phoneNumber
            });

            attributes.Add(new AttributeType
            {
                Name = "phone_number_verified",
                Value = "true"
            });
        }

        try
        {
            var existingUser = await FindUserByAliasAsync(phoneNumber, false, cancellationToken);

            if (existingUser is not null) return;
        }
        catch (UserNotFoundException)
        {
            //continue and create
        }

        await _cognito.AdminCreateUserAsync(new AdminCreateUserRequest
        {
            UserPoolId = _options.UserPoolId,
            Username = username,
            TemporaryPassword = password,
            MessageAction = MessageActionType.SUPPRESS,
            UserAttributes = attributes
        }, cancellationToken);

        await _cognito.AdminSetUserPasswordAsync(new AdminSetUserPasswordRequest
        {
            UserPoolId = _options.UserPoolId,
            Username = username,
            Password = password,
            Permanent = true
        }, cancellationToken);
    }

    private async Task<UserType?> FindUserByAliasAsync(
        string loginValue,
        bool isCognitoUserName = true,
        CancellationToken cancellationToken = default
    )
    {
        loginValue = OtpHashHelper.NormalizeUsername(loginValue);

        var filterName = isCognitoUserName == true ? "username" : IsEmail(loginValue)
            ? "email"
            : "phone_number";

        var response = await _cognito.ListUsersAsync(new ListUsersRequest
        {
            UserPoolId = _options.UserPoolId,
            Filter = $"{filterName} = \"{loginValue}\"",
            Limit = 1
        }, cancellationToken);

        return response.Users.FirstOrDefault();
    }

    private static bool IsEmail(string username) => username.Contains('@');

    public async Task<LoginOtpStartResponse> StartCustomAuthAsync(
        string username,
        bool isCognitoUserName = true,
        CancellationToken cancellationToken = default
    )
    {
        username = OtpHashHelper.NormalizeUsername(username);

        var user = await FindUserByAliasAsync(username, isCognitoUserName, cancellationToken);

        if (user is null)
            return new LoginOtpStartResponse
            {
                code = "1",
                message = ResponseCodes.CodeCUE105_User_not_found
            };

        var cognitoUsername = user.Username;

        var authParameters = new Dictionary<string, string>
        {
            ["USERNAME"] = cognitoUsername,
        };

        AddSecretHashIfRequired(authParameters, cognitoUsername);

        var response = await _cognito.AdminInitiateAuthAsync(
            new AdminInitiateAuthRequest
            {
                UserPoolId = _options.UserPoolId,
                ClientId = _options.ClientId,
                AuthFlow = AuthFlowType.CUSTOM_AUTH,
                AuthParameters = authParameters
            },
            cancellationToken
        );

        if (string.IsNullOrEmpty(response.Session))
            return new LoginOtpStartResponse
            {
                code = "1",
                message = ResponseCodes.CodeCUE105_User_not_found
            };

        return new LoginOtpStartResponse
        {
            code = "0",
            message = ResponseCodes.Code200_Success,
            challengeName = response.ChallengeName?.Value ?? "CUSTOM_CHALLENGE",
            session = response.Session
        };
    }

    public async Task<AuthTokensResponse> ResponseToCustomAuthChallengeAync(
        string username,
        string session,
        string otpCode,
        CancellationToken cancellationToken = default)
    {
        string latestSession = string.Empty;

        username = OtpHashHelper.NormalizeUsername(username);

        var challengeResponses = new Dictionary<string, string>
        {
            ["USERNAME"] = username,
            ["ANSWER"] = otpCode
        };

        AddSecretHashIfRequired(challengeResponses, username);

        var response = await _cognito.AdminRespondToAuthChallengeAsync(
            new AdminRespondToAuthChallengeRequest
            {
                UserPoolId = _options.UserPoolId,
                ClientId = _options.ClientId,
                ChallengeName = ChallengeNameType.CUSTOM_CHALLENGE,
                Session = session,
                ChallengeResponses = challengeResponses
            }, cancellationToken
        );

        if (!string.IsNullOrEmpty(response.Session))
            latestSession = response.Session;

        if (response.AuthenticationResult is null)
            return new AuthTokensResponse
            {
                code = "1",
                message = ResponseCodes.CUERES134_ReservationInvalidOTP,
                latestSession = latestSession
            };

        return new AuthTokensResponse
        {
            code = "0",
            message = ResponseCodes.Code200_Success,
            accessToken = response.AuthenticationResult.AccessToken ?? string.Empty,
            idToken = response.AuthenticationResult.IdToken ?? string.Empty,
            refreshToken = response.AuthenticationResult.RefreshToken ?? string.Empty,
            expiresIn = response.AuthenticationResult.ExpiresIn,
            tokenType = response.AuthenticationResult.TokenType ?? "Bearer"
        };
    }

    public async Task<bool> EnsureCustomerExistsAsync(
        string username,
        CancellationToken cancellationToken = default
    )
    {
        username = OtpHashHelper.NormalizeUsername(username);

        try
        {
            await _cognito.AdminGetUserAsync(new AdminGetUserRequest
            {
                UserPoolId = _options.UserPoolId,
                Username = username
            }, cancellationToken);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private void AddSecretHashIfRequired(
        Dictionary<string, string> parameters,
        string username
    )
    {
        if (string.IsNullOrWhiteSpace(_options.ClientSecret))
            return;

        parameters["SECRET_HASH"] = CognitoSecretHashHelper.CalculateSecretHash(
            username,
            _options.ClientId,
            _options.ClientSecret);
    }

    public sealed class LoginOtpStartResponse
    {
        public string? code { get; set; }
        public string? message { get; set; }
        public string? username { get; set; }
        public string? challengeName { get; set; }
        public string? session { get; set; }
    }

    public sealed class AuthTokensResponse
    {
        public string? code { get; set; }
        public string? message { get; set; }
        public string? latestSession { get; set; }
        public string? accessToken { get; set; }
        public string? idToken { get; set; }
        public string? refreshToken { get; set; }
        public int? expiresIn { get; set; }
        public string? tokenType { get; set; }
    }
}