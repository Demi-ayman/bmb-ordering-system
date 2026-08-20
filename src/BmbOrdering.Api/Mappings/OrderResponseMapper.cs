using BmbOrdering.Api.Contracts.Orders;
using BmbOrdering.Application.Orders.Common;

namespace BmbOrdering.Api.Mappings;

public static class OrderResponseMapper
{
    public static OrderResponse Map(OrderResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var items = result.Items
            .Select(item => new OrderItemResponse(
                item.Id,
                item.ProductName,
                item.Quantity,
                item.UnitPrice,
                item.LineTotal))
            .ToArray();

        return new OrderResponse(
            result.Id,
            result.CustomerId,
            result.OrderNumber,
            result.Status,
            result.TotalAmount,
            result.CreatedAtUtc,
            result.DeletedAtUtc,
            items);
    }
}
