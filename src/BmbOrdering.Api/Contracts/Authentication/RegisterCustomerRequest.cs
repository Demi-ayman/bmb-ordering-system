namespace BmbOrdering.Api.Contracts.Authentication;

public sealed record RegisterCustomerRequest(
    string FullName,
    string Email,
    string Password,
    string PasswordConfirmation);