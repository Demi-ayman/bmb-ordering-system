namespace BmbOrdering.Application.Authentication.Register;

public sealed record RegisterCustomerResult(
    Guid CustomerId,
    string FullName,
    string Email,
    DateTime CreatedAtUtc);