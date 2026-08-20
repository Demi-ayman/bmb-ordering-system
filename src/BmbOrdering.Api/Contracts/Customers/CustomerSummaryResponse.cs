namespace BmbOrdering.Api.Contracts.Customers;

public sealed record CustomerSummaryResponse(
    Guid Id,
    string FullName,
    string Email,
    DateTime CreatedAtUtc,
    DateTime? BannedUntilUtc,
    bool IsOrderingBanned);
