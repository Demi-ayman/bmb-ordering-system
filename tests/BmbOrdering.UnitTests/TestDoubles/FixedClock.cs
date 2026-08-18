using BmbOrdering.Application.Abstractions.Time;

namespace BmbOrdering.UnitTests.TestDoubles;

public sealed class FixedClock : IClock
{
    public FixedClock(DateTime utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTime UtcNow { get; set; }
}