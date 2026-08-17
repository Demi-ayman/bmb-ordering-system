using BmbOrdering.Domain.Common;

namespace BmbOrdering.Domain.Orders;

public sealed class OrderDeletionEvent
{
    private OrderDeletionEvent()
    {
    }

    private OrderDeletionEvent(
        Guid id,
        Guid orderId,
        Guid customerId,
        DateTime orderCreatedAtUtc,
        DateTime deletedAtUtc,
        bool qualifiesForBanCount)
    {
        Id = id;
        OrderId = orderId;
        CustomerId = customerId;
        OrderCreatedAtUtc = orderCreatedAtUtc;
        DeletedAtUtc = deletedAtUtc;
        QualifiesForBanCount = qualifiesForBanCount;
    }

    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public Guid CustomerId { get; private set; }

    public DateTime OrderCreatedAtUtc { get; private set; }

    public DateTime DeletedAtUtc { get; private set; }

    public bool QualifiesForBanCount { get; private set; }

    public static OrderDeletionEvent Record(Order order)
    {
        if (order is null)
        {
            throw new DomainException("Order is required.");
        }

        if (!order.IsDeleted || !order.DeletedAtUtc.HasValue)
        {
            throw new DomainException(
                "A deletion event can only be recorded for a deleted order.");
        }

        return new OrderDeletionEvent(
            Guid.NewGuid(),
            order.Id,
            order.CustomerId,
            order.CreatedAtUtc,
            order.DeletedAtUtc.Value,
            order.WasDeletedOnCreationDate());
    }
}