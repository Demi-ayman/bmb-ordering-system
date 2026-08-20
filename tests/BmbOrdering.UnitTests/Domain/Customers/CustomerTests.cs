using BmbOrdering.Domain.Common;
using BmbOrdering.Domain.Customers;

namespace BmbOrdering.UnitTests.Domain.Customers;

public sealed class CustomerTests
{
    private static readonly DateTime UtcNow =
        new(2026, 8, 17, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Register_WithValidValues_CreatesCustomer()
    {
        var customer = CreateCustomer();

        Assert.NotEqual(Guid.Empty, customer.Id);
        Assert.Equal("Demiana Ayman", customer.FullName);
        Assert.Equal("demiana@example.com", customer.Email);
        Assert.Equal("DEMIANA@EXAMPLE.COM", customer.NormalizedEmail);
        Assert.Equal("hashed-password", customer.PasswordHash);
        Assert.Equal(UtcNow, customer.CreatedAtUtc);
        Assert.Null(customer.BannedUntilUtc);
    }

    [Fact]
    public void Register_TrimsCustomerValues()
    {
        var customer = Customer.Register(
            "  Demiana Ayman  ",
            "  demiana@example.com  ",
            "  DEMIANA@EXAMPLE.COM  ",
            "hashed-password",
            UtcNow);

        Assert.Equal("Demiana Ayman", customer.FullName);
        Assert.Equal("demiana@example.com", customer.Email);
        Assert.Equal("DEMIANA@EXAMPLE.COM", customer.NormalizedEmail);
    }

    [Fact]
    public void Register_WithoutFullName_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(
            () => Customer.Register(
                string.Empty,
                "demiana@example.com",
                "DEMIANA@EXAMPLE.COM",
                "hashed-password",
                UtcNow));

        Assert.Equal("Customer full name is required.", exception.Message);
    }

    [Fact]
    public void ApplyOrderingBan_BansCustomerForSixHours()
    {
        var customer = CreateCustomer();

        customer.ApplyOrderingBan(UtcNow);

        Assert.Equal(UtcNow.AddHours(6), customer.BannedUntilUtc);
        Assert.True(customer.IsOrderingBannedAt(UtcNow.AddHours(5)));
        Assert.False(customer.IsOrderingBannedAt(UtcNow.AddHours(6)));
    }

    [Fact]
    public void ApplyOrderingBan_WhenBanIsActive_DoesNotExtendBan()
    {
        var customer = CreateCustomer();

        customer.ApplyOrderingBan(UtcNow);
        customer.ApplyOrderingBan(UtcNow.AddHours(1));

        Assert.Equal(UtcNow.AddHours(6), customer.BannedUntilUtc);
    }

    [Fact]
    public void IsOrderingBannedAt_WithNonUtcValue_ThrowsDomainException()
    {
        var customer = CreateCustomer();
        var localTime = DateTime.SpecifyKind(UtcNow, DateTimeKind.Local);

        var exception = Assert.Throws<DomainException>(
            () => customer.IsOrderingBannedAt(localTime));

        Assert.Equal(
            "utcNow must use the UTC date and time kind.",
            exception.Message);
    }

    private static Customer CreateCustomer()
    {
        return Customer.Register(
            "Demiana Ayman",
            "demiana@example.com",
            "DEMIANA@EXAMPLE.COM",
            "hashed-password",
            UtcNow);
    }
}