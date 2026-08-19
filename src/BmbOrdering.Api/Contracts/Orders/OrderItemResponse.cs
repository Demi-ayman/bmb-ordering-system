namespace BmbOrdering.Api.Contracts.Orders;

public sealed record OrderItemResponse(
    Guid Id,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);