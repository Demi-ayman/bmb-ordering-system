namespace BmbOrdering.Application.Orders.Delete;

public sealed record DeleteOrderResult(
	Guid OrderId,
	DateTime DeletedAtUtc,
	bool QualifiesForBanCount,
	int QualifyingDeletionCount,
	DateTime? BannedUntilUtc);