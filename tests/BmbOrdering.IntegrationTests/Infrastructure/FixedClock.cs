using BmbOrdering.Application.Abstractions.Time;

namespace BmbOrdering.IntegrationTests.Infrastructure;

public sealed class FixedClock : IClock
{
    public FixedClock(DateTime utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTime UtcNow { get; set; }
}
