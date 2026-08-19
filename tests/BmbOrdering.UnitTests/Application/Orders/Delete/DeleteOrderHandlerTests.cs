using BmbOrdering.Application.Common.Exceptions;
using BmbOrdering.Application.Orders.Delete;
using BmbOrdering.Domain.Customers;
using BmbOrdering.Domain.Orders;
using BmbOrdering.UnitTests.TestDoubles;

namespace BmbOrdering.UnitTests.Application.Orders.Delete;

public sealed class DeleteOrderHandlerTests
{
    private static readonly DateTime UtcNow =
        new(2026, 8, 19, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task HandleAsync_WithOwnedOrder_DeletesAndRecordsEvent()
    {
        var fixture = CreateFixture();
        var order = CreateOrder(
            fixture.Customer.Id,
            "ORD-1",
            UtcNow.AddHours(-1));

        fixture.OrderRepository.Seed(order);

        var result = await fixture.Handler.HandleAsync(
            new DeleteOrderCommand(order.Id));

        var deletionEvent = Assert.Single(
            fixture.DeletionEventRepository.Events);

        Assert.True(order.IsDeleted);
        Assert.Equal(UtcNow, order.DeletedAtUtc);
        Assert.Equal(order.Id, deletionEvent.OrderId);
        Assert.True(deletionEvent.QualifiesForBanCount);
        Assert.Equal(1, result.QualifyingDeletionCount);
        Assert.Null(result.BannedUntilUtc);
        Assert.Equal(1, fixture.UnitOfWork.SaveChangesCallCount);
        Assert.Equal(1, fixture.TransactionManager.ExecutionCount);
    }

    [Fact]
    public async Task HandleAsync_OnThirdQualifyingDeletion_BansCustomerForSixHours()
    {
        var fixture = CreateFixture();
        var orders = new[]
        {
            CreateOrder(fixture.Customer.Id, "ORD-1", UtcNow.AddHours(-3)),
            CreateOrder(fixture.Customer.Id, "ORD-2", UtcNow.AddHours(-2)),
            CreateOrder(fixture.Customer.Id, "ORD-3", UtcNow.AddHours(-1))
        };

        foreach (var order in orders)
        {
            fixture.OrderRepository.Seed(order);
        }

        DeleteOrderResult? result = null;

        foreach (var order in orders)
        {
            result = await fixture.Handler.HandleAsync(
                new DeleteOrderCommand(order.Id));
        }

        var finalResult = Assert.IsType<DeleteOrderResult>(result);

        Assert.Equal(3, finalResult.QualifyingDeletionCount);
        Assert.Equal(UtcNow.AddHours(6), finalResult.BannedUntilUtc);
        Assert.True(fixture.Customer.IsOrderingBannedAt(UtcNow));
        Assert.Equal(3, fixture.DeletionEventRepository.Events.Count);
        Assert.All(orders, order => Assert.True(order.IsDeleted));
        Assert.Equal(4, fixture.UnitOfWork.SaveChangesCallCount);
        Assert.Equal(3, fixture.TransactionManager.ExecutionCount);
    }

    [Fact]
    public async Task HandleAsync_WhenOrderWasCreatedOnPreviousDay_DoesNotCountDeletion()
    {
        var fixture = CreateFixture();
        var order = CreateOrder(
            fixture.Customer.Id,
            "ORD-OLD",
            UtcNow.AddDays(-1));

        fixture.OrderRepository.Seed(order);

        var result = await fixture.Handler.HandleAsync(
            new DeleteOrderCommand(order.Id));

        Assert.False(result.QualifiesForBanCount);
        Assert.Equal(0, result.QualifyingDeletionCount);
        Assert.Null(result.BannedUntilUtc);
        Assert.False(fixture.Customer.IsOrderingBannedAt(UtcNow));
    }

    [Fact]
    public async Task HandleAsync_WithAnotherCustomersOrder_ThrowsNotFoundException()
    {
        var fixture = CreateFixture();
        var anotherCustomer = CreateCustomer(
            "Another Customer",
            "another@example.com");

        var order = CreateOrder(
            anotherCustomer.Id,
            "ORD-OTHER",
            UtcNow.AddHours(-1));

        fixture.CustomerRepository.Seed(anotherCustomer);
        fixture.OrderRepository.Seed(order);

        await Assert.ThrowsAsync<NotFoundException>(
            () => fixture.Handler.HandleAsync(
                new DeleteOrderCommand(order.Id)));

        Assert.False(order.IsDeleted);
        Assert.Empty(fixture.DeletionEventRepository.Events);
        Assert.Equal(0, fixture.UnitOfWork.SaveChangesCallCount);
    }

    private static DeleteFixture CreateFixture()
    {
        var customer = CreateCustomer(
            "Demiana Ayman",
            "demiana@example.com");

        var currentUser = new FakeCurrentUserContext
        {
            IsAuthenticated = true,
            CustomerId = customer.Id
        };

        var customerRepository = new FakeCustomerRepository();
        var orderRepository = new FakeOrderRepository();
        var deletionEventRepository =
            new FakeOrderDeletionEventRepository();
        var unitOfWork = new FakeUnitOfWork();
        var transactionManager = new FakeTransactionManager();
        var clock = new FixedClock(UtcNow);

        customerRepository.Seed(customer);

        var handler = new DeleteOrderHandler(
            new DeleteOrderValidator(),
            currentUser,
            customerRepository,
            orderRepository,
            deletionEventRepository,
            unitOfWork,
            transactionManager,
            clock);

        return new DeleteFixture(
            handler,
            customer,
            customerRepository,
            orderRepository,
            deletionEventRepository,
            unitOfWork,
            transactionManager);
    }

    private static Customer CreateCustomer(
        string fullName,
        string email)
    {
        return Customer.Register(
            fullName,
            email,
            email.ToUpperInvariant(),
            "password-hash",
            UtcNow.AddDays(-2));
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

    private sealed record DeleteFixture(
        DeleteOrderHandler Handler,
        Customer Customer,
        FakeCustomerRepository CustomerRepository,
        FakeOrderRepository OrderRepository,
        FakeOrderDeletionEventRepository DeletionEventRepository,
        FakeUnitOfWork UnitOfWork,
        FakeTransactionManager TransactionManager);
}
