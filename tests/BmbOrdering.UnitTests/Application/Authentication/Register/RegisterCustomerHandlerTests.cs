using BmbOrdering.Application.Authentication.Register;
using BmbOrdering.Application.Common.Exceptions;
using BmbOrdering.Domain.Customers;
using BmbOrdering.UnitTests.TestDoubles;

namespace BmbOrdering.UnitTests.Application.Authentication.Register;

public sealed class RegisterCustomerHandlerTests
{
    private static readonly DateTime UtcNow =
        new(2026, 8, 17, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task HandleAsync_WithValidCommand_CreatesCustomer()
    {
        var repository = new FakeCustomerRepository();
        var passwordService = new FakePasswordService();
        var unitOfWork = new FakeUnitOfWork();
        var clock = new FixedClock(UtcNow);
        var handler = CreateHandler(
            repository,
            passwordService,
            unitOfWork,
            clock);

        var command = CreateValidCommand();

        var result = await handler.HandleAsync(command);

        var customer = Assert.Single(repository.Customers);

        Assert.Equal(customer.Id, result.CustomerId);
        Assert.Equal("Demiana Ayman", result.FullName);
        Assert.Equal("demiana@example.com", result.Email);
        Assert.Equal(UtcNow, result.CreatedAtUtc);

        Assert.Equal("DEMIANA@EXAMPLE.COM", customer.NormalizedEmail);
        Assert.Equal("HASH::StrongPass1", customer.PasswordHash);
        Assert.Equal("StrongPass1", passwordService.LastHashedPassword);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_WithExistingEmail_ThrowsConflictException()
    {
        var repository = new FakeCustomerRepository();
        var passwordService = new FakePasswordService();
        var unitOfWork = new FakeUnitOfWork();
        var clock = new FixedClock(UtcNow);

        repository.Seed(
            Customer.Register(
                "Existing Customer",
                "demiana@example.com",
                "DEMIANA@EXAMPLE.COM",
                "existing-hash",
                UtcNow));

        var handler = CreateHandler(
            repository,
            passwordService,
            unitOfWork,
            clock);

        var command = new RegisterCustomerCommand(
            "Demiana Ayman",
            "DEMIANA@example.com",
            "StrongPass1",
            "StrongPass1");

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => handler.HandleAsync(command));

        Assert.Equal(
            "A customer with this email address already exists.",
            exception.Message);

        Assert.Single(repository.Customers);
        Assert.Null(passwordService.LastHashedPassword);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    private static RegisterCustomerHandler CreateHandler(
        FakeCustomerRepository repository,
        FakePasswordService passwordService,
        FakeUnitOfWork unitOfWork,
        FixedClock clock)
    {
        return new RegisterCustomerHandler(
            new RegisterCustomerValidator(),
            repository,
            passwordService,
            unitOfWork,
            clock);
    }

    private static RegisterCustomerCommand CreateValidCommand()
    {
        return new RegisterCustomerCommand(
            "Demiana Ayman",
            "demiana@example.com",
            "StrongPass1",
            "StrongPass1");
    }
}