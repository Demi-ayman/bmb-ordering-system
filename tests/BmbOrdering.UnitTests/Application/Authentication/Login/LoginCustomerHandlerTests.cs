using BmbOrdering.Application.Authentication.Login;
using BmbOrdering.Application.Common.Authorization;
using BmbOrdering.Application.Common.Exceptions;
using BmbOrdering.Domain.Customers;
using BmbOrdering.UnitTests.TestDoubles;

namespace BmbOrdering.UnitTests.Application.Authentication.Login;

public sealed class LoginCustomerHandlerTests
{
    private static readonly DateTime UtcNow =
        new(2026, 8, 18, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task HandleAsync_WithValidCredentials_ReturnsAccessToken()
    {
        var repository = new FakeCustomerRepository();
        var passwordService = new FakePasswordService();
        var tokenGenerator = new FakeJwtTokenGenerator(
            "generated-access-token",
            UtcNow.AddMinutes(30));

        var customer = CreateCustomer();
        customer.ApplyOrderingBan(UtcNow);
        repository.Seed(customer);

        var handler = CreateHandler(
            repository,
            passwordService,
            tokenGenerator);

        var command = new LoginCustomerCommand(
            "DEMIANA@example.com",
            "StrongPass1");

        var result = await handler.HandleAsync(command);

        Assert.Equal(customer.Id, result.CustomerId);
        Assert.Equal(customer.FullName, result.FullName);
        Assert.Equal(customer.Email, result.Email);
        Assert.Equal("generated-access-token", result.AccessToken);
        Assert.Equal(
            UtcNow.AddMinutes(30),
            result.AccessTokenExpiresAtUtc);
        Assert.Equal(
            UtcNow.AddHours(6),
            result.BannedUntilUtc);

        Assert.Same(customer, tokenGenerator.LastCustomer);
        Assert.NotNull(tokenGenerator.LastRoles);
        Assert.Contains(
            RoleNames.Customer,
            tokenGenerator.LastRoles);
    }

    [Fact]
    public async Task HandleAsync_ForAdministrator_IncludesAdministratorRole()
    {
        var repository = new FakeCustomerRepository();
        var passwordService = new FakePasswordService();
        var tokenGenerator = new FakeJwtTokenGenerator(
            "administrator-token",
            UtcNow.AddMinutes(30));
        var roleProvider = new FakeUserRoleProvider(
            RoleNames.Customer,
            RoleNames.Administrator);

        var customer = CreateCustomer();
        repository.Seed(customer);

        var handler = CreateHandler(
            repository,
            passwordService,
            tokenGenerator,
            roleProvider);

        await handler.HandleAsync(
            new LoginCustomerCommand(
                "demiana@example.com",
                "StrongPass1"));

        Assert.Same(customer, roleProvider.LastCustomer);
        Assert.NotNull(tokenGenerator.LastRoles);
        Assert.Contains(
            RoleNames.Administrator,
            tokenGenerator.LastRoles);
    }

    [Fact]
    public async Task HandleAsync_WithUnknownEmail_ThrowsInvalidCredentials()
    {
        var repository = new FakeCustomerRepository();
        var passwordService = new FakePasswordService();
        var tokenGenerator = new FakeJwtTokenGenerator(
            "unused-token",
            UtcNow.AddMinutes(30));

        var handler = CreateHandler(
            repository,
            passwordService,
            tokenGenerator);

        var command = new LoginCustomerCommand(
            "unknown@example.com",
            "StrongPass1");

        var exception =
            await Assert.ThrowsAsync<InvalidCredentialsException>(
                () => handler.HandleAsync(command));

        Assert.Equal(
            "The email address or password is invalid.",
            exception.Message);

        Assert.Null(tokenGenerator.LastCustomer);
    }

    [Fact]
    public async Task HandleAsync_WithIncorrectPassword_ThrowsSameSafeError()
    {
        var repository = new FakeCustomerRepository();
        var passwordService = new FakePasswordService();
        var tokenGenerator = new FakeJwtTokenGenerator(
            "unused-token",
            UtcNow.AddMinutes(30));

        repository.Seed(CreateCustomer());

        var handler = CreateHandler(
            repository,
            passwordService,
            tokenGenerator);

        var command = new LoginCustomerCommand(
            "demiana@example.com",
            "WrongPassword1");

        var exception =
            await Assert.ThrowsAsync<InvalidCredentialsException>(
                () => handler.HandleAsync(command));

        Assert.Equal(
            "The email address or password is invalid.",
            exception.Message);

        Assert.Null(tokenGenerator.LastCustomer);
    }

    private static LoginCustomerHandler CreateHandler(
        FakeCustomerRepository repository,
        FakePasswordService passwordService,
        FakeJwtTokenGenerator tokenGenerator,
        FakeUserRoleProvider? roleProvider = null)
    {
        return new LoginCustomerHandler(
            new LoginCustomerValidator(),
            repository,
            passwordService,
            tokenGenerator,
            roleProvider ?? new FakeUserRoleProvider(
                RoleNames.Customer));
    }

    private static Customer CreateCustomer()
    {
        return Customer.Register(
            "Demiana Ayman",
            "demiana@example.com",
            "DEMIANA@EXAMPLE.COM",
            "HASH::StrongPass1",
            UtcNow);
    }
}
