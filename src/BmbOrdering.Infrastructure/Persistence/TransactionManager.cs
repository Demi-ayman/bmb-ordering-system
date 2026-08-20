using System.Data;
using BmbOrdering.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BmbOrdering.Infrastructure.Persistence;

public sealed class TransactionManager :
    ITransactionManager
{
    private readonly OrderingDbContext _dbContext;

    public TransactionManager(
        OrderingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TResult>
        ExecuteSerializableAsync<TResult>(
            Func<CancellationToken, Task<TResult>> operation,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var executionStrategy =
            _dbContext.Database.CreateExecutionStrategy();

        return await executionStrategy.ExecuteAsync(
            async () =>
            {
                await using var transaction =
                    await _dbContext.Database
                        .BeginTransactionAsync(
                            IsolationLevel.Serializable,
                            cancellationToken);

                try
                {
                    var result =
                        await operation(cancellationToken);

                    await transaction.CommitAsync(
                        cancellationToken);

                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync(
                        CancellationToken.None);

                    throw;
                }
            });
    }
}