using BmbOrdering.Application.Abstractions.Persistence;
using BmbOrdering.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace BmbOrdering.Infrastructure.Persistence.Repositories;

public sealed class OrderDeletionEventRepository :
    IOrderDeletionEventRepository
{
    private readonly OrderingDbContext _dbContext;

    public OrderDeletionEventRepository(
        OrderingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsForOrderAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.OrderDeletionEvents.AnyAsync(
            deletionEvent =>
                deletionEvent.OrderId == orderId,
            cancellationToken);
    }

    public Task<int> CountQualifyingDeletionsAsync(
        Guid customerId,
        DateTime dayStartUtc,
        DateTime dayEndUtc,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.OrderDeletionEvents.CountAsync(
            deletionEvent =>
                deletionEvent.CustomerId == customerId &&
                deletionEvent.QualifiesForBanCount &&
                deletionEvent.DeletedAtUtc >= dayStartUtc &&
                deletionEvent.DeletedAtUtc < dayEndUtc,
            cancellationToken);
    }

    public void Add(
        OrderDeletionEvent deletionEvent)
    {
        _dbContext.OrderDeletionEvents.Add(
            deletionEvent);
    }
}