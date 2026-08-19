using BmbOrdering.Application.Abstractions.Authorization;
using BmbOrdering.Application.Common.Authorization;
using BmbOrdering.Domain.Customers;
using Microsoft.Extensions.Options;

namespace BmbOrdering.Infrastructure.Authorization;

public sealed class ConfigurationUserRoleProvider :
    IUserRoleProvider
{
    private readonly AuthorizationOptions _options;

    public ConfigurationUserRoleProvider(
        IOptions<AuthorizationOptions> options)
    {
        _options = options.Value;
    }

    public IReadOnlyCollection<string> GetRoles(
        Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);

        var isAdministrator =
            _options.AdministratorEmails.Any(email =>
                string.Equals(
                    email?.Trim(),
                    customer.Email,
                    StringComparison.OrdinalIgnoreCase));

        return isAdministrator
            ? new[]
            {
                RoleNames.Customer,
                RoleNames.Administrator
            }
            : new[] { RoleNames.Customer };
    }
}
