namespace BmbOrdering.Application.Orders.Common;

public sealed record OrderItemResult(
	Guid Id,
	string ProductName,
	int Quantity,
	decimal UnitPrice,
	decimal LineTotal);