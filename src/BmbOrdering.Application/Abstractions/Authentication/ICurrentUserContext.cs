namespace BmbOrdering.Application.Abstractions.Authentication;

public interface ICurrentUserContext
{
    bool IsAuthenticated { get; }

    Guid? CustomerId { get; }

    IReadOnlyCollection<string> Roles { get; }
}