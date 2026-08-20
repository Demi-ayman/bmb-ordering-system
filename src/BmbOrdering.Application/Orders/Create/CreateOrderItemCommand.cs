namespace BmbOrdering.Application.Orders.Create;

public sealed record CreateOrderItemCommand(
    string ProductName,
    int Quantity,
    decimal UnitPrice);