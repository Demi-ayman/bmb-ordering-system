using BmbOrdering.Domain.Orders;

namespace BmbOrdering.Application.Abstractions.Persistence;

public interface IOrderRepository
{
    Task<Order?> GetByIdForCustomerAsync(
        Guid orderId,
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Order>> GetAllAsync(
        CancellationToken cancellationToken = default);

    void Add(Order order);
}