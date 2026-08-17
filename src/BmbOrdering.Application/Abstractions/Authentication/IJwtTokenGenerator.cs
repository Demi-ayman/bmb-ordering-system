using BmbOrdering.Domain.Customers;

namespace BmbOrdering.Application.Abstractions.Authentication;

public interface IJwtTokenGenerator
{
    AccessToken GenerateToken(
        Customer customer,
        IReadOnlyCollection<string> roles);
}