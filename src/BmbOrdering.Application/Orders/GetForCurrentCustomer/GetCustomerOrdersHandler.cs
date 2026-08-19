using BmbOrdering.Application.Abstractions.Authentication;
using BmbOrdering.Application.Abstractions.Persistence;
using BmbOrdering.Application.Common.Exceptions;
using BmbOrdering.Application.Orders.Common;

namespace BmbOrdering.Application.Orders.GetForCurrentCustomer;

public sealed class GetCustomerOrdersHandler
{
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IOrderRepository _orderRepository;

    public GetCustomerOrdersHandler(
        ICurrentUserContext currentUserContext,
        IOrderRepository orderRepository)
    {
        _currentUserContext = currentUserContext;
        _orderRepository = orderRepository;
    }

    public async Task<IReadOnlyList<OrderResult>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var customerId = GetAuthenticatedCustomerId();

        var orders =
            await _orderRepository.GetByCustomerIdAsync(
                customerId,
                cancellationToken);

        return orders
            .Select(OrderResultMapper.Map)
            .ToArray();
    }

    private Guid GetAuthenticatedCustomerId()
    {
        if (!_currentUserContext.IsAuthenticated ||
            !_currentUserContext.CustomerId.HasValue ||
            _currentUserContext.CustomerId.Value == Guid.Empty)
        {
            throw new AuthenticationRequiredException();
        }

        return _currentUserContext.CustomerId.Value;
    }
}