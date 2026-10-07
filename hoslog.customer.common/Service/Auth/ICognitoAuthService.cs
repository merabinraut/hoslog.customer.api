using static hoslog.customer.common.Service.CognitoAuthService;

namespace hoslog.customer.common.Service;

public interface ICognitoAuthService
{
    Task CreateConfirmedCustomerUserAsync(
      string username,
      string fullName,
      string password,
      string email,
      string phoneNumber,
      CancellationToken cancellationToken = default
  );

    Task<LoginOtpStartResponse> StartCustomAuthAsync(
        string username,
        bool isCognitoUserName = true,
        CancellationToken cancellationToken = default
    );

    Task<AuthTokensResponse> ResponseToCustomAuthChallengeAync(
       string username,
       string session,
       string otpCode,
       CancellationToken cancellationToken = default);

    Task<bool> EnsureCustomerExistsAsync(
        string username,
        CancellationToken cancellationToken = default
    );
}