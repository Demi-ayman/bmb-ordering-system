using BmbOrdering.Domain.Orders;

namespace BmbOrdering.Application.Orders.Common;

public static class OrderResultMapper
{
    public static OrderResult Map(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        var items = order.Items
            .Select(item => new OrderItemResult(
                item.Id,
                item.ProductName,
                item.Quantity,
                item.UnitPrice,
                item.LineTotal))
            .ToArray();

        return new OrderResult(
            order.Id,
            order.CustomerId,
            order.OrderNumber,
            order.Status.ToString(),
            order.TotalAmount,
            order.CreatedAtUtc,
            order.DeletedAtUtc,
            items);
    }
}