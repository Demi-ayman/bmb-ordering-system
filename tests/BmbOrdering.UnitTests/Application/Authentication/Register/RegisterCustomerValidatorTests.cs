using BmbOrdering.Application.Authentication.Register;
using BmbOrdering.Application.Common.Exceptions;

namespace BmbOrdering.UnitTests.Application.Authentication.Register;

public sealed class RegisterCustomerValidatorTests
{
    private readonly RegisterCustomerValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_DoesNotThrow()
    {
        var command = CreateValidCommand();

        var exception = Record.Exception(
            () => _validator.Validate(command));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_WithInvalidFields_ReturnsAllRelevantErrors()
    {
        var command = new RegisterCustomerCommand(
            string.Empty,
            "invalid-email",
            "short",
            "different");

        var exception = Assert.Throws<ValidationException>(
            () => _validator.Validate(command));

        Assert.Contains(
            nameof(RegisterCustomerCommand.FullName),
            exception.Errors.Keys);

        Assert.Contains(
            nameof(RegisterCustomerCommand.Email),
            exception.Errors.Keys);

        Assert.Contains(
            nameof(RegisterCustomerCommand.Password),
            exception.Errors.Keys);

        Assert.Contains(
            nameof(RegisterCustomerCommand.PasswordConfirmation),
            exception.Errors.Keys);
    }

    [Fact]
    public void Validate_WithMismatchedConfirmation_ReturnsConfirmationError()
    {
        var command = new RegisterCustomerCommand(
            "Demiana Ayman",
            "demiana@example.com",
            "StrongPass1",
            "StrongPass2");

        var exception = Assert.Throws<ValidationException>(
            () => _validator.Validate(command));

        var errors =
            exception.Errors[
                nameof(RegisterCustomerCommand.PasswordConfirmation)];

        Assert.Contains(
            "Password confirmation does not match.",
            errors);
    }

    private static RegisterCustomerCommand CreateValidCommand()
    {
        return new RegisterCustomerCommand(
            "Demiana Ayman",
            "demiana@example.com",
            "StrongPass1",
            "StrongPass1");
    }
}