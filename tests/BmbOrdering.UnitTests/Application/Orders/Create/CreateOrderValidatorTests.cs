using BmbOrdering.Application.Common.Exceptions;
using BmbOrdering.Application.Orders.Create;

namespace BmbOrdering.UnitTests.Application.Orders.Create;

public sealed class CreateOrderValidatorTests
{
    private readonly CreateOrderValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_DoesNotThrow()
    {
        var command = CreateValidCommand();

        _validator.Validate(command);
    }

    [Fact]
    public void Validate_WithNoItems_ThrowsValidationException()
    {
        var command = new CreateOrderCommand(
            Array.Empty<CreateOrderItemCommand>());

        var exception = Assert.Throws<ValidationException>(
            () => _validator.Validate(command));

        Assert.Contains(
            "An order must contain at least one item.",
            exception.Errors[nameof(CreateOrderCommand.Items)]);
    }

    [Fact]
    public void Validate_WithTooManyItems_ThrowsValidationException()
    {
        var items = Enumerable
            .Range(1, 101)
            .Select(_ => new CreateOrderItemCommand(
                "Product",
                1,
                10.00m))
            .ToArray();

        var command = new CreateOrderCommand(items);

        var exception = Assert.Throws<ValidationException>(
            () => _validator.Validate(command));

        Assert.Contains(
            "An order cannot contain more than 100 items.",
            exception.Errors[nameof(CreateOrderCommand.Items)]);
    }

    [Fact]
    public void Validate_WithInvalidItem_ReturnsItemSpecificErrors()
    {
        var command = new CreateOrderCommand(
            new[]
            {
                new CreateOrderItemCommand(
                    " ",
                    0,
                    -1.00m)
            });

        var exception = Assert.Throws<ValidationException>(
            () => _validator.Validate(command));

        Assert.Contains(
            "Product name is required.",
            exception.Errors["Items[0].ProductName"]);

        Assert.Contains(
            "Quantity must be greater than zero.",
            exception.Errors["Items[0].Quantity"]);

        Assert.Contains(
            "Unit price cannot be negative.",
            exception.Errors["Items[0].UnitPrice"]);
    }

    [Fact]
    public void Validate_WithMoreThanTwoDecimalPlaces_ThrowsValidationException()
    {
        var command = new CreateOrderCommand(
            new[]
            {
                new CreateOrderItemCommand(
                    "Product",
                    1,
                    10.123m)
            });

        var exception = Assert.Throws<ValidationException>(
            () => _validator.Validate(command));

        Assert.Contains(
            "Unit price cannot contain more than two decimal places.",
            exception.Errors["Items[0].UnitPrice"]);
    }

    private static CreateOrderCommand CreateValidCommand()
    {
        return new CreateOrderCommand(
            new[]
            {
                new CreateOrderItemCommand(
                    "Keyboard",
                    2,
                    750.50m),
                new CreateOrderItemCommand(
                    "Mouse",
                    1,
                    250.00m)
            });
    }
}