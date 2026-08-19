using BmbOrdering.Application.Abstractions.Authorization;
using BmbOrdering.Domain.Customers;

namespace BmbOrdering.UnitTests.TestDoubles;

public sealed class FakeUserRoleProvider : IUserRoleProvider
{
    private readonly IReadOnlyCollection<string> _roles;

    public FakeUserRoleProvider(
        params string[] roles)
    {
        _roles = roles;
    }

    public Customer? LastCustomer { get; private set; }

    public IReadOnlyCollection<string> GetRoles(
        Customer customer)
    {
        LastCustomer = customer;

        return _roles;
    }
}
