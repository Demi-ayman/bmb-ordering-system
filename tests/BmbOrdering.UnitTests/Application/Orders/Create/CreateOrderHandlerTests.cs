using BmbOrdering.Application.Common.Exceptions;
using BmbOrdering.Application.Orders.Create;
using BmbOrdering.Domain.Customers;
using BmbOrdering.UnitTests.TestDoubles;

namespace BmbOrdering.UnitTests.Application.Orders.Create;

public sealed class CreateOrderHandlerTests
{
    private static readonly DateTime UtcNow =
        new(2026, 8, 19, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task HandleAsync_WithValidCommand_CreatesOrder()
    {
        var customer = CreateCustomer();
        var customerRepository = new FakeCustomerRepository();
        var orderRepository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var clock = new FixedClock(UtcNow);
        var currentUser = CreateAuthenticatedUser(customer.Id);

        customerRepository.Seed(customer);

        var handler = CreateHandler(
            currentUser,
            customerRepository,
            orderRepository,
            unitOfWork,
            clock);

        var result = await handler.HandleAsync(CreateValidCommand());

        var order = Assert.Single(orderRepository.Orders);

        Assert.Equal(order.Id, result.Id);
        Assert.Equal(customer.Id, result.CustomerId);
        Assert.Equal("Created", result.Status);
        Assert.Equal(1751.00m, result.TotalAmount);
        Assert.Equal(UtcNow, result.CreatedAtUtc);
        Assert.Null(result.DeletedAtUtc);
        Assert.Equal(2, result.Items.Count);

        Assert.StartsWith(
            "ORD-20260819120000-",
            result.OrderNumber);

        Assert.Equal(27, result.OrderNumber.Length);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_UsesAuthenticatedCustomerForOwnership()
    {
        var firstCustomer = CreateCustomer(
            "First Customer",
            "first@example.com");

        var authenticatedCustomer = CreateCustomer(
            "Authenticated Customer",
            "authenticated@example.com");

        var customerRepository = new FakeCustomerRepository();
        var orderRepository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var clock = new FixedClock(UtcNow);

        customerRepository.Seed(firstCustomer);
        customerRepository.Seed(authenticatedCustomer);

        var currentUser =
            CreateAuthenticatedUser(authenticatedCustomer.Id);

        var handler = CreateHandler(
            currentUser,
            customerRepository,
            orderRepository,
            unitOfWork,
            clock);

        var result = await handler.HandleAsync(CreateValidCommand());

        Assert.Equal(
            authenticatedCustomer.Id,
            result.CustomerId);

        Assert.NotEqual(
            firstCustomer.Id,
            result.CustomerId);
    }

    [Fact]
    public async Task HandleAsync_WhenUnauthenticated_ThrowsAuthenticationRequiredException()
    {
        var customerRepository = new FakeCustomerRepository();
        var orderRepository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var clock = new FixedClock(UtcNow);

        var currentUser = new FakeCurrentUserContext
        {
            IsAuthenticated = false,
            CustomerId = null
        };

        var handler = CreateHandler(
            currentUser,
            customerRepository,
            orderRepository,
            unitOfWork,
            clock);

        await Assert.ThrowsAsync<AuthenticationRequiredException>(
            () => handler.HandleAsync(CreateValidCommand()));

        Assert.Empty(orderRepository.Orders);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerDoesNotExist_ThrowsNotFoundException()
    {
        var customerRepository = new FakeCustomerRepository();
        var orderRepository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var clock = new FixedClock(UtcNow);

        var currentUser =
            CreateAuthenticatedUser(Guid.NewGuid());

        var handler = CreateHandler(
            currentUser,
            customerRepository,
            orderRepository,
            unitOfWork,
            clock);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => handler.HandleAsync(CreateValidCommand()));

        Assert.Equal(
            "The authenticated customer was not found.",
            exception.Message);

        Assert.Empty(orderRepository.Orders);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_WhenCustomerIsBanned_ThrowsOrderingBannedException()
    {
        var customer = CreateCustomer();

        customer.ApplyOrderingBan(
            UtcNow.AddHours(-1));

        var customerRepository = new FakeCustomerRepository();
        var orderRepository = new FakeOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var clock = new FixedClock(UtcNow);
        var currentUser = CreateAuthenticatedUser(customer.Id);

        customerRepository.Seed(customer);

        var handler = CreateHandler(
            currentUser,
            customerRepository,
            orderRepository,
            unitOfWork,
            clock);

        var exception =
            await Assert.ThrowsAsync<OrderingBannedException>(
                () => handler.HandleAsync(CreateValidCommand()));

        Assert.Equal(
            UtcNow.AddHours(5),
            exception.BannedUntilUtc);

        Assert.Empty(orderRepository.Orders);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    private static CreateOrderHandler CreateHandler(
        FakeCurrentUserContext currentUser,
        FakeCustomerRepository customerRepository,
        FakeOrderRepository orderRepository,
        FakeUnitOfWork unitOfWork,
        FixedClock clock)
    {
        return new CreateOrderHandler(
            new CreateOrderValidator(),
            currentUser,
            customerRepository,
            orderRepository,
            unitOfWork,
            clock);
    }

    private static FakeCurrentUserContext CreateAuthenticatedUser(
        Guid customerId)
    {
        return new FakeCurrentUserContext
        {
            IsAuthenticated = true,
            CustomerId = customerId
        };
    }

    private static Customer CreateCustomer(
        string fullName = "Demiana Ayman",
        string email = "demiana@example.com")
    {
        return Customer.Register(
            fullName,
            email,
            email.ToUpperInvariant(),
            "password-hash",
            UtcNow.AddDays(-1));
    }

    private static CreateOrderCommand CreateValidCommand()
    {
        return new CreateOrderCommand(
            new[]
            {
                new CreateOrderItemCommand(
                    "Keyboard",
                    2,
                    750.50m),
                new CreateOrderItemCommand(
                    "Mouse",
                    1,
                    250.00m)
            });
    }
}