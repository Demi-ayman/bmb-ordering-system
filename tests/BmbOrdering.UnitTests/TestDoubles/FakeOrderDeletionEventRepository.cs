using BmbOrdering.Application.Abstractions.Persistence;
using BmbOrdering.Domain.Orders;

namespace BmbOrdering.UnitTests.TestDoubles;

public sealed class FakeOrderDeletionEventRepository :
    IOrderDeletionEventRepository
{
    private readonly List<OrderDeletionEvent> _events = new();

    public IReadOnlyCollection<OrderDeletionEvent> Events =>
        _events.AsReadOnly();

    public Task<bool> ExistsForOrderAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            _events.Any(deletionEvent =>
                deletionEvent.OrderId == orderId));
    }

    public Task<int> CountQualifyingDeletionsAsync(
        Guid customerId,
        DateTime dayStartUtc,
        DateTime dayEndUtc,
        CancellationToken cancellationToken = default)
    {
        var count = _events.Count(deletionEvent =>
            deletionEvent.CustomerId == customerId &&
            deletionEvent.QualifiesForBanCount &&
            deletionEvent.DeletedAtUtc >= dayStartUtc &&
            deletionEvent.DeletedAtUtc < dayEndUtc);

        return Task.FromResult(count);
    }

    public void Add(OrderDeletionEvent deletionEvent)
    {
        _events.Add(deletionEvent);
    }
}
