namespace BmbOrdering.Application.Orders.Common;

public sealed record OrderResult(
    Guid Id,
    Guid CustomerId,
    string OrderNumber,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAtUtc,
    DateTime? DeletedAtUtc,
    IReadOnlyCollection<OrderItemResult> Items);