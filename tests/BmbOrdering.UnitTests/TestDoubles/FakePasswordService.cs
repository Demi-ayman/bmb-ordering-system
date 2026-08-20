using BmbOrdering.Application.Abstractions.Authentication;

namespace BmbOrdering.UnitTests.TestDoubles;

public sealed class FakePasswordService : IPasswordService
{
    private const string HashPrefix = "HASH::";

    public string? LastHashedPassword { get; private set; }

    public string HashPassword(string password)
    {
        LastHashedPassword = password;

        return $"{HashPrefix}{password}";
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return string.Equals(
            passwordHash,
            $"{HashPrefix}{password}",
            StringComparison.Ordinal);
    }
}