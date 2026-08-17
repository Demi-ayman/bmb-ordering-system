using BmbOrdering.Domain.Orders;

namespace BmbOrdering.Application.Abstractions.Persistence;

public interface IOrderDeletionEventRepository
{
    Task<bool> ExistsForOrderAsync(
        Guid orderId,
        CancellationToken cancellationToken = default);

    Task<int> CountQualifyingDeletionsAsync(
        Guid customerId,
        DateTime dayStartUtc,
        DateTime dayEndUtc,
        CancellationToken cancellationToken = default);

    void Add(OrderDeletionEvent deletionEvent);
}