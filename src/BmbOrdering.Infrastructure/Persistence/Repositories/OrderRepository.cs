using BmbOrdering.Application.Abstractions.Persistence;
using BmbOrdering.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace BmbOrdering.Infrastructure.Persistence.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly OrderingDbContext _dbContext;

    public OrderRepository(
        OrderingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Order?> GetByIdForCustomerAsync(
        Guid orderId,
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Orders
            .Include(order => order.Items)
            .SingleOrDefaultAsync(
                order =>
                    order.Id == orderId &&
                    order.CustomerId == customerId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Order>>
        GetByCustomerIdAsync(
            Guid customerId,
            CancellationToken cancellationToken = default)
    {
        var orders = await _dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Items)
            .Where(order => order.CustomerId == customerId)
            .OrderByDescending(order => order.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return orders;
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var orders = await _dbContext.Orders
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(order => order.Items)
            .OrderByDescending(order => order.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return orders;
    }

    public void Add(Order order)
    {
        _dbContext.Orders.Add(order);
    }
}