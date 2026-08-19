namespace BmbOrdering.Api.Contracts.Orders;

public sealed record DeleteOrderResponse(
    Guid OrderId,
    DateTime DeletedAtUtc,
    bool QualifiesForBanCount,
    int QualifyingDeletionCount,
    DateTime? BannedUntilUtc);