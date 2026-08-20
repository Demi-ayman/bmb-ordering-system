namespace BmbOrdering.Application.Customers.GetAll;

public sealed record CustomerSummaryResult(
    Guid Id,
    string FullName,
    string Email,
    DateTime CreatedAtUtc,
    DateTime? BannedUntilUtc,
    bool IsOrderingBanned);
