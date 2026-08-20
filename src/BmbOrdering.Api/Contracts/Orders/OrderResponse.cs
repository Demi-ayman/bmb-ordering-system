namespace BmbOrdering.Api.Contracts.Orders;

public sealed record OrderResponse(
	Guid Id,
	Guid CustomerId,
	string OrderNumber,
	string Status,
	decimal TotalAmount,
	DateTime CreatedAtUtc,
	DateTime? DeletedAtUtc,
	IReadOnlyCollection<OrderItemResponse> Items);