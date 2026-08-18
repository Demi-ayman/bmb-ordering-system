namespace BmbOrdering.Api.Contracts.Authentication;

public sealed record RegisteredCustomerResponse(
    Guid CustomerId,
    string FullName,
    string Email,
    DateTime CreatedAtUtc);