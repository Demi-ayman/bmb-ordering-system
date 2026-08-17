namespace BmbOrdering.Application.Abstractions.Time;

public interface IClock
{
    DateTime UtcNow { get; }
}