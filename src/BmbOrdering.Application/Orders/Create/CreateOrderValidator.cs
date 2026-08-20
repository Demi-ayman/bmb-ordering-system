using BmbOrdering.Application.Common.Exceptions;
using BmbOrdering.Domain.Orders;

namespace BmbOrdering.Application.Orders.Create;

public sealed class CreateOrderValidator
{
    private const int MaximumItemCount = 100;

    public void Validate(CreateOrderCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        var errors = new Dictionary<string, List<string>>();

        if (command.Items is null || command.Items.Count == 0)
        {
            AddError(
                errors,
                nameof(CreateOrderCommand.Items),
                "An order must contain at least one item.");
        }
        else
        {
            if (command.Items.Count > MaximumItemCount)
            {
                AddError(
                    errors,
                    nameof(CreateOrderCommand.Items),
                    $"An order cannot contain more than {MaximumItemCount} items.");
            }

            ValidateItems(command.Items, errors);
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(
                errors.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.ToArray()));
        }
    }

    private static void ValidateItems(
        IEnumerable<CreateOrderItemCommand> items,
        IDictionary<string, List<string>> errors)
    {
        var index = 0;

        foreach (var item in items)
        {
            var propertyPrefix =
                $"{nameof(CreateOrderCommand.Items)}[{index}]";

            if (item is null)
            {
                AddError(
                    errors,
                    propertyPrefix,
                    "Order item is required.");

                index++;
                continue;
            }

            ValidateProductName(item, propertyPrefix, errors);
            ValidateQuantity(item, propertyPrefix, errors);
            ValidateUnitPrice(item, propertyPrefix, errors);

            index++;
        }
    }

    private static void ValidateProductName(
        CreateOrderItemCommand item,
        string propertyPrefix,
        IDictionary<string, List<string>> errors)
    {
        var propertyName =
            $"{propertyPrefix}.{nameof(CreateOrderItemCommand.ProductName)}";

        if (string.IsNullOrWhiteSpace(item.ProductName))
        {
            AddError(errors, propertyName, "Product name is required.");
            return;
        }

        if (item.ProductName.Trim().Length > OrderItem.ProductNameMaxLength)
        {
            AddError(
                errors,
                propertyName,
                $"Product name cannot exceed {OrderItem.ProductNameMaxLength} characters.");
        }
    }

    private static void ValidateQuantity(
        CreateOrderItemCommand item,
        string propertyPrefix,
        IDictionary<string, List<string>> errors)
    {
        if (item.Quantity <= 0)
        {
            AddError(
                errors,
                $"{propertyPrefix}.{nameof(CreateOrderItemCommand.Quantity)}",
                "Quantity must be greater than zero.");
        }
    }

    private static void ValidateUnitPrice(
        CreateOrderItemCommand item,
        string propertyPrefix,
        IDictionary<string, List<string>> errors)
    {
        var propertyName =
            $"{propertyPrefix}.{nameof(CreateOrderItemCommand.UnitPrice)}";

        if (item.UnitPrice < 0)
        {
            AddError(
                errors,
                propertyName,
                "Unit price cannot be negative.");

            return;
        }

        if (decimal.Round(item.UnitPrice, 2) != item.UnitPrice)
        {
            AddError(
                errors,
                propertyName,
                "Unit price cannot contain more than two decimal places.");
        }
    }

    private static void AddError(
        IDictionary<string, List<string>> errors,
        string propertyName,
        string message)
    {
        if (!errors.TryGetValue(propertyName, out var propertyErrors))
        {
            propertyErrors = new List<string>();
            errors[propertyName] = propertyErrors;
        }

        propertyErrors.Add(message);
    }
}