using BmbOrdering.Domain.Customers;

namespace BmbOrdering.Application.Abstractions.Authorization;

public interface IUserRoleProvider
{
    IReadOnlyCollection<string> GetRoles(Customer customer);
}
