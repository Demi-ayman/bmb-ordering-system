namespace BmbOrdering.Application.Authentication.Register;

public sealed record RegisterCustomerCommand(
    string FullName,
    string Email,
    string Password,
    string PasswordConfirmation);