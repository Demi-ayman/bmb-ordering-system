using BmbOrdering.Application.Common.Exceptions;
using BmbOrdering.Application.Customers.GetOrders;
using BmbOrdering.Domain.Customers;
using BmbOrdering.Domain.Orders;
using BmbOrdering.UnitTests.TestDoubles;

namespace BmbOrdering.UnitTests.Application.Customers.GetOrders;

public sealed class GetCustomerOrdersForAdminHandlerTests
{
    private static readonly DateTime UtcNow =
        new(2026, 8, 20, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task HandleAsync_ReturnsSelectedCustomersActiveAndDeletedOrders()
    {
        var customerRepository = new FakeCustomerRepository();
        var orderRepository = new FakeOrderRepository();
        var customer = CreateCustomer(
            "Selected Customer",
            "selected@example.com");
        var anotherCustomer = CreateCustomer(
            "Another Customer",
            "another@example.com");

        customerRepository.Seed(customer);
        customerRepository.Seed(anotherCustomer);

        var olderOrder = CreateOrder(
            customer.Id,
            "ORD-OLDER",
            UtcNow.AddHours(-2));
        var newerDeletedOrder = CreateOrder(
            customer.Id,
            "ORD-NEWER",
            UtcNow.AddHours(-1));
        newerDeletedOrder.Delete(UtcNow);
        var unrelatedOrder = CreateOrder(
            anotherCustomer.Id,
            "ORD-OTHER",
            UtcNow.AddHours(-3));

        orderRepository.Seed(olderOrder);
        orderRepository.Seed(newerDeletedOrder);
        orderRepository.Seed(unrelatedOrder);

        var handler = CreateHandler(
            customerRepository,
            orderRepository);

        var results = await handler.HandleAsync(
            new GetCustomerOrdersForAdminQuery(customer.Id));

        Assert.Equal(2, results.Count);
        Assert.Equal(newerDeletedOrder.Id, results[0].Id);
        Assert.Equal("Deleted", results[0].Status);
        Assert.Equal(olderOrder.Id, results[1].Id);
        Assert.Equal("Created", results[1].Status);
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerDoesNotExist_ThrowsNotFoundException()
    {
        var handler = CreateHandler(
            new FakeCustomerRepository(),
            new FakeOrderRepository());

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(
                new GetCustomerOrdersForAdminQuery(Guid.NewGuid())));

        Assert.Equal(
            "The requested customer was not found.",
            exception.Message);
    }

    private static GetCustomerOrdersForAdminHandler CreateHandler(
        FakeCustomerRepository customerRepository,
        FakeOrderRepository orderRepository)
    {
        return new GetCustomerOrdersForAdminHandler(
            new GetCustomerOrdersForAdminValidator(),
            customerRepository,
            orderRepository);
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
            UtcNow.AddDays(-1));
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
