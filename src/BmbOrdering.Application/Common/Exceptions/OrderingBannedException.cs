namespace BmbOrdering.Application.Common.Exceptions;

public sealed class OrderingBannedException : Exception
{
	public OrderingBannedException(DateTime bannedUntilUtc)
		: base(
			$"Customer cannot place new orders until " +
			$"{bannedUntilUtc:O}.")
	{
		BannedUntilUtc = bannedUntilUtc;
	}

	public DateTime BannedUntilUtc { get; }
}