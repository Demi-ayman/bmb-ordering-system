namespace BmbOrdering.Application.Orders.Create;

public sealed record CreateOrderCommand(
	IReadOnlyCollection<CreateOrderItemCommand> Items);