namespace BmbOrdering.Application.Abstractions.Persistence;

public interface ITransactionManager
{
    Task<TResult> ExecuteSerializableAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default);
}