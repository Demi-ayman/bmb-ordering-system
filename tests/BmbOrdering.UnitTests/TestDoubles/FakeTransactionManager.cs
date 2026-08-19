using BmbOrdering.Application.Abstractions.Persistence;

namespace BmbOrdering.UnitTests.TestDoubles;

public sealed class FakeTransactionManager : ITransactionManager
{
    public int ExecutionCount { get; private set; }

    public Task<TResult> ExecuteSerializableAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        ExecutionCount++;

        return operation(cancellationToken);
    }
}
