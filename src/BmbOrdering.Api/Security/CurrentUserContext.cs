using System.Security.Claims;
using BmbOrdering.Application.Abstractions.Authentication;

namespace BmbOrdering.Api.Security;

public sealed class CurrentUserContext :
    ICurrentUserContext
{
    private readonly IHttpContextAccessor
        _httpContextAccessor;

    public CurrentUserContext(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor =
            httpContextAccessor;
    }

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated == true;

    public Guid? CustomerId
    {
        get
        {
            var value = User?.FindFirstValue(
                ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var customerId)
                ? customerId
                : null;
        }
    }

    public IReadOnlyCollection<string> Roles =>
        User?
            .FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray()
        ?? Array.Empty<string>();

    private ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;
}