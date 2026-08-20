namespace BmbOrdering.Application.Abstractions.Authentication;

public sealed record AccessToken(
    string Value,
    DateTime ExpiresAtUtc);