using BmbOrdering.Domain.Common;
using BmbOrdering.Domain.Orders;

namespace BmbOrdering.UnitTests.Domain.Orders;

public sealed class OrderTests
{
    private static readonly Guid CustomerId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly DateTime CreatedAtUtc =
        new(2026, 8, 17, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_WithValidItems_CreatesOrderAndCalculatesTotal()
    {
        var order = CreateOrder();

        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.Equal(CustomerId, order.CustomerId);
        Assert.Equal("ORD-20260817-001", order.OrderNumber);
        Assert.Equal(OrderStatus.Created, order.Status);
        Assert.Equal(65.50m, order.TotalAmount);
        Assert.Equal(CreatedAtUtc, order.CreatedAtUtc);
        Assert.Null(order.DeletedAtUtc);
        Assert.False(order.IsDeleted);

        Assert.Collection(
            order.Items,
            firstItem =>
            {
                Assert.NotEqual(Guid.Empty, firstItem.Id);
                Assert.Equal(order.Id, firstItem.OrderId);
                Assert.Equal("Keyboard", firstItem.ProductName);
                Assert.Equal(2, firstItem.Quantity);
                Assert.Equal(25.00m, firstItem.UnitPrice);
                Assert.Equal(50.00m, firstItem.LineTotal);
            },
            secondItem =>
            {
                Assert.NotEqual(Guid.Empty, secondItem.Id);
                Assert.Equal(order.Id, secondItem.OrderId);
                Assert.Equal("Mouse", secondItem.ProductName);
                Assert.Equal(1, secondItem.Quantity);
                Assert.Equal(15.50m, secondItem.UnitPrice);
                Assert.Equal(15.50m, secondItem.LineTotal);
            });
    }

    [Fact]
    public void Create_WithoutItems_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => Order.Create(
                CustomerId,
                "ORD-20260817-001",
                Array.Empty<OrderItemDetails>(),
                CreatedAtUtc));

        Assert.Equal(
            "An order must contain at least one item.",
            exception.Message);
    }

    [Fact]
    public void Create_WithInvalidItemQuantity_ThrowsDomainException()
    {
        var item = new OrderItemDetails(
            "Keyboard",
            0,
            25.00m);

        var exception = Assert.Throws<DomainException>(
            () => Order.Create(
                CustomerId,
                "ORD-20260817-001",
                new[] { item },
                CreatedAtUtc));

        Assert.Equal(
            "Order item quantity must be greater than zero.",
            exception.Message);
    }

    [Fact]
    public void Delete_WithValidTime_SoftDeletesOrder()
    {
        var order = CreateOrder();
        var deletedAtUtc = CreatedAtUtc.AddHours(2);

        order.Delete(deletedAtUtc);

        Assert.True(order.IsDeleted);
        Assert.Equal(OrderStatus.Deleted, order.Status);
        Assert.Equal(deletedAtUtc, order.DeletedAtUtc);
    }

    [Fact]
    public void Delete_WhenOrderIsAlreadyDeleted_ThrowsDomainException()
    {
        var order = CreateOrder();
        order.Delete(CreatedAtUtc.AddHours(1));

        var exception = Assert.Throws<DomainException>(
            () => order.Delete(CreatedAtUtc.AddHours(2)));

        Assert.Equal("Order is already deleted.", exception.Message);
    }

    [Fact]
    public void WasDeletedOnCreationDate_WhenDeletedSameDay_ReturnsTrue()
    {
        var order = CreateOrder();
        order.Delete(CreatedAtUtc.AddHours(3));

        var result = order.WasDeletedOnCreationDate();

        Assert.True(result);
    }

    [Fact]
    public void WasDeletedOnCreationDate_WhenDeletedNextDay_ReturnsFalse()
    {
        var order = CreateOrder();
        var nextDayUtc = CreatedAtUtc.Date.AddDays(1);

        order.Delete(nextDayUtc);

        var result = order.WasDeletedOnCreationDate();

        Assert.False(result);
    }

    [Fact]
    public void Delete_WhenTimeIsBeforeCreation_ThrowsDomainException()
    {
        var order = CreateOrder();

        var exception = Assert.Throws<DomainException>(
            () => order.Delete(CreatedAtUtc.AddSeconds(-1)));

        Assert.Equal(
            "Order deletion time cannot be before its creation time.",
            exception.Message);
    }

    private static Order CreateOrder()
    {
        var items = new[]
        {
            new OrderItemDetails(
                "Keyboard",
                2,
                25.00m),
            new OrderItemDetails(
                "Mouse",
                1,
                15.50m)
        };

        return Order.Create(
            CustomerId,
            "ORD-20260817-001",
            items,
            CreatedAtUtc);
    }
}