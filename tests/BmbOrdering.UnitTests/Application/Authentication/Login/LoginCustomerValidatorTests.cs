using BmbOrdering.Application.Authentication.Login;
using BmbOrdering.Application.Common.Exceptions;

namespace BmbOrdering.UnitTests.Application.Authentication.Login;

public sealed class LoginCustomerValidatorTests
{
    private readonly LoginCustomerValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_DoesNotThrow()
    {
        var command = new LoginCustomerCommand(
            "demiana@example.com",
            "StrongPass1");

        var exception = Record.Exception(
            () => _validator.Validate(command));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_WithMissingCredentials_ReturnsFieldErrors()
    {
        var command = new LoginCustomerCommand(
            string.Empty,
            string.Empty);

        var exception = Assert.Throws<ValidationException>(
            () => _validator.Validate(command));

        Assert.Contains(
            nameof(LoginCustomerCommand.Email),
            exception.Errors.Keys);

        Assert.Contains(
            nameof(LoginCustomerCommand.Password),
            exception.Errors.Keys);
    }
}