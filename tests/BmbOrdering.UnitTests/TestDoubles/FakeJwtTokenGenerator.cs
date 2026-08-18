using BmbOrdering.Application.Abstractions.Authentication;
using BmbOrdering.Domain.Customers;

namespace BmbOrdering.UnitTests.TestDoubles;

public sealed class FakeJwtTokenGenerator : IJwtTokenGenerator
{
    public FakeJwtTokenGenerator(
        string tokenValue,
        DateTime expiresAtUtc)
    {
        TokenValue = tokenValue;
        ExpiresAtUtc = expiresAtUtc;
    }

    public string TokenValue { get; }

    public DateTime ExpiresAtUtc { get; }

    public Customer? LastCustomer { get; private set; }

    public IReadOnlyCollection<string>? LastRoles { get; private set; }

    public AccessToken GenerateToken(
        Customer customer,
        IReadOnlyCollection<string> roles)
    {
        LastCustomer = customer;
        LastRoles = roles;

        return new AccessToken(
            TokenValue,
            ExpiresAtUtc);
    }
}