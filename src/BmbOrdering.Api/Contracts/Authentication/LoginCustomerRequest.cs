namespace BmbOrdering.Api.Contracts.Authentication;

public sealed record LoginCustomerRequest(
    string Email,
    string Password);