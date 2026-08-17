using BmbOrdering.Domain.Common;

namespace BmbOrdering.Domain.Customers;

public sealed class Customer
{
    public const int FullNameMaxLength = 150;
    public const int EmailMaxLength = 256;

    private static readonly TimeSpan OrderingBanDuration = TimeSpan.FromHours(6);

    private Customer()
    {
        FullName = string.Empty;
        Email = string.Empty;
        NormalizedEmail = string.Empty;
        PasswordHash = string.Empty;
    }

    private Customer(
        Guid id,
        string fullName,
        string email,
        string normalizedEmail,
        string passwordHash,
        DateTime createdAtUtc)
    {
        Id = id;
        FullName = fullName;
        Email = email;
        NormalizedEmail = normalizedEmail;
        PasswordHash = passwordHash;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public string FullName { get; private set; }

    public string Email { get; private set; }

    public string NormalizedEmail { get; private set; }

    public string PasswordHash { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? BannedUntilUtc { get; private set; }

    public static Customer Register(
        string fullName,
        string email,
        string normalizedEmail,
        string passwordHash,
        DateTime createdAtUtc)
    {
        EnsureUtc(createdAtUtc, nameof(createdAtUtc));

        fullName = RequireValue(
            fullName,
            FullNameMaxLength,
            "Customer full name is required.",
            "Customer full name cannot exceed 150 characters.");

        email = RequireValue(
            email,
            EmailMaxLength,
            "Customer email is required.",
            "Customer email cannot exceed 256 characters.");

        normalizedEmail = RequireValue(
            normalizedEmail,
            EmailMaxLength,
            "Normalized customer email is required.",
            "Normalized customer email cannot exceed 256 characters.");

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("Customer password hash is required.");
        }

        return new Customer(
            Guid.NewGuid(),
            fullName,
            email,
            normalizedEmail,
            passwordHash,
            createdAtUtc);
    }

    public bool IsOrderingBannedAt(DateTime utcNow)
    {
        EnsureUtc(utcNow, nameof(utcNow));

        return BannedUntilUtc.HasValue &&
               BannedUntilUtc.Value > utcNow;
    }

    public void ApplyOrderingBan(DateTime utcNow)
    {
        EnsureUtc(utcNow, nameof(utcNow));

        if (IsOrderingBannedAt(utcNow))
        {
            return;
        }

        BannedUntilUtc = utcNow.Add(OrderingBanDuration);
    }

    private static string RequireValue(
        string value,
        int maximumLength,
        string requiredMessage,
        string maximumLengthMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(requiredMessage);
        }

        value = value.Trim();

        if (value.Length > maximumLength)
        {
            throw new DomainException(maximumLengthMessage);
        }

        return value;
    }

    private static void EnsureUtc(DateTime value, string parameterName)
    {
        if (value.Kind != DateTimeKind.Utc)
        {
            throw new DomainException(
                $"{parameterName} must use the UTC date and time kind.");
        }
    }
}