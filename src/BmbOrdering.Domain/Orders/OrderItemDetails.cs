namespace BmbOrdering.Domain.Orders;

public readonly record struct OrderItemDetails(
    string ProductName,
    int Quantity,
    decimal UnitPrice);