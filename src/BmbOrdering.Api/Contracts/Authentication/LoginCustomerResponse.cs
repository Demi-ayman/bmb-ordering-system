namespace BmbOrdering.Api.Contracts.Authentication;

public sealed record LoginCustomerResponse(
	Guid CustomerId,
	string FullName,
	string Email,
	string AccessToken,
	DateTime AccessTokenExpiresAtUtc,
	DateTime? BannedUntilUtc);