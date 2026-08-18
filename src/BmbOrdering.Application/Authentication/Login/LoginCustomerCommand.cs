namespace BmbOrdering.Application.Authentication.Login;

public sealed record LoginCustomerCommand(
    string Email,
    string Password);