using BmbOrdering.Application.Abstractions.Authentication;

namespace BmbOrdering.UnitTests.TestDoubles;

public sealed class FakeCurrentUserContext : ICurrentUserContext
{
	public bool IsAuthenticated { get; set; }

	public Guid? CustomerId { get; set; }

	public IReadOnlyCollection<string> Roles { get; set; } =
		Array.Empty<string>();
}