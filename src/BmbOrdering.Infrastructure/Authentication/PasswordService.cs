using BmbOrdering.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Identity;

namespace BmbOrdering.Infrastructure.Authentication;

public sealed class PasswordService : IPasswordService
{
    private static readonly object PasswordHasherUser = new();

    private readonly PasswordHasher<object> _passwordHasher = new();

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException(
                "Password is required.",
                nameof(password));
        }

        return _passwordHasher.HashPassword(
            PasswordHasherUser,
            password);
    }

    public bool VerifyPassword(
        string password,
        string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        var result = _passwordHasher.VerifyHashedPassword(
            PasswordHasherUser,
            passwordHash,
            password);

        return result is
            PasswordVerificationResult.Success or
            PasswordVerificationResult.SuccessRehashNeeded;
    }
}