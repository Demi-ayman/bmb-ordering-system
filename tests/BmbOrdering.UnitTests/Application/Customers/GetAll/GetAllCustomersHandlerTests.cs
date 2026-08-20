using BmbOrdering.Application.Customers.GetAll;
using BmbOrdering.Domain.Customers;
using BmbOrdering.UnitTests.TestDoubles;

namespace BmbOrdering.UnitTests.Application.Customers.GetAll;

public sealed class GetAllCustomersHandlerTests
{
    private static readonly DateTime UtcNow =
        new(2026, 8, 20, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task HandleAsync_ReturnsCustomersOrderedByNameWithBanStatus()
    {
        var repository = new FakeCustomerRepository();
        var clock = new FixedClock(UtcNow);

        var zara = CreateCustomer(
            "Zara Customer",
            "zara@example.com");
        zara.ApplyOrderingBan(UtcNow);

        var adam = CreateCustomer(
            "Adam Customer",
            "adam@example.com");

        repository.Seed(zara);
        repository.Seed(adam);

        var handler = new GetAllCustomersHandler(
            repository,
            clock);

        var results = await handler.HandleAsync();

        Assert.Equal(2, results.Count);
        Assert.Equal(adam.Id, results[0].Id);
        Assert.False(results[0].IsOrderingBanned);
        Assert.Equal(zara.Id, results[1].Id);
        Assert.True(results[1].IsOrderingBanned);
        Assert.Equal(UtcNow.AddHours(6), results[1].BannedUntilUtc);
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
}
