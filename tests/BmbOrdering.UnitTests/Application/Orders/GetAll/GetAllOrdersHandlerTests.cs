using BmbOrdering.Application.Orders.GetAll;
using BmbOrdering.Domain.Orders;
using BmbOrdering.UnitTests.TestDoubles;

namespace BmbOrdering.UnitTests.Application.Orders.GetAll;

public sealed class GetAllOrdersHandlerTests
{
    private static readonly DateTime UtcNow =
        new(2026, 8, 19, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task HandleAsync_ReturnsActiveAndDeletedOrdersNewestFirst()
    {
        var repository = new FakeOrderRepository();
        var customerId = Guid.NewGuid();

        var olderOrder = CreateOrder(
            customerId,
            "ORD-OLDER",
            UtcNow.AddHours(-2));

        var newerDeletedOrder = CreateOrder(
            customerId,
            "ORD-NEWER",
            UtcNow.AddHours(-1));

        newerDeletedOrder.Delete(UtcNow);

        repository.Seed(olderOrder);
        repository.Seed(newerDeletedOrder);

        var handler = new GetAllOrdersHandler(repository);

        var results = await handler.HandleAsync();

        Assert.Equal(2, results.Count);
        Assert.Equal(newerDeletedOrder.Id, results[0].Id);
        Assert.Equal("Deleted", results[0].Status);
        Assert.Equal(olderOrder.Id, results[1].Id);
        Assert.Equal("Created", results[1].Status);
    }

    private static Order CreateOrder(
        Guid customerId,
        string orderNumber,
        DateTime createdAtUtc)
    {
        return Order.Create(
            customerId,
            orderNumber,
            new[]
            {
                new OrderItemDetails("Keyboard", 1, 100.00m)
            },
            createdAtUtc);
    }
}
