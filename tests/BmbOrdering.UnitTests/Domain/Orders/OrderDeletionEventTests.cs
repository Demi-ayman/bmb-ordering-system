using BmbOrdering.Domain.Common;
using BmbOrdering.Domain.Orders;

namespace BmbOrdering.UnitTests.Domain.Orders;

public sealed class OrderDeletionEventTests
{
    private static readonly Guid CustomerId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly DateTime CreatedAtUtc =
        new(2026, 8, 17, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Record_ForSameDayDeletedOrder_CreatesQualifyingEvent()
    {
        var order = CreateOrder();
        var deletedAtUtc = CreatedAtUtc.AddHours(2);
        order.Delete(deletedAtUtc);

        var deletionEvent = OrderDeletionEvent.Record(order);

        Assert.NotEqual(Guid.Empty, deletionEvent.Id);
        Assert.Equal(order.Id, deletionEvent.OrderId);
        Assert.Equal(CustomerId, deletionEvent.CustomerId);
        Assert.Equal(CreatedAtUtc, deletionEvent.OrderCreatedAtUtc);
        Assert.Equal(deletedAtUtc, deletionEvent.DeletedAtUtc);
        Assert.True(deletionEvent.QualifiesForBanCount);
    }

    [Fact]
    public void Record_ForNextDayDeletedOrder_CreatesNonQualifyingEvent()
    {
        var order = CreateOrder();
        order.Delete(CreatedAtUtc.Date.AddDays(1));

        var deletionEvent = OrderDeletionEvent.Record(order);

        Assert.False(deletionEvent.QualifiesForBanCount);
    }

    [Fact]
    public void Record_ForActiveOrder_ThrowsDomainException()
    {
        var order = CreateOrder();

        var exception = Assert.Throws<DomainException>(
            () => OrderDeletionEvent.Record(order));

        Assert.Equal(
            "A deletion event can only be recorded for a deleted order.",
            exception.Message);
    }

    private static Order CreateOrder()
    {
        var items = new[]
        {
            new OrderItemDetails(
                "Monitor",
                1,
                250.00m)
        };

        return Order.Create(
            CustomerId,
            "ORD-20260817-002",
            items,
            CreatedAtUtc);
    }
}