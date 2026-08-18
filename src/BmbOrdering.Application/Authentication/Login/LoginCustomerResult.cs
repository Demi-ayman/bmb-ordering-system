namespace BmbOrdering.Application.Authentication.Login;

public sealed record LoginCustomerResult(
    Guid CustomerId,
    string FullName,
    string Email,
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime? BannedUntilUtc);