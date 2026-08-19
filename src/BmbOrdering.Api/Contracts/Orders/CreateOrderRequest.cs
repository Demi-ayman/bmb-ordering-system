namespace BmbOrdering.Api.Contracts.Orders;

public sealed record CreateOrderRequest(
    IReadOnlyCollection<CreateOrderItemRequest> Items);