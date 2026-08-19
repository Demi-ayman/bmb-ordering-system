using BmbOrdering.Application.Abstractions.Authentication;
using BmbOrdering.Application.Abstractions.Persistence;
using BmbOrdering.Application.Common.Exceptions;
using BmbOrdering.Application.Orders.Common;

namespace BmbOrdering.Application.Orders.GetById;

public sealed class GetOrderByIdHandler
{
    private readonly GetOrderByIdValidator _validator;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdHandler(
        GetOrderByIdValidator validator,
        ICurrentUserContext currentUserContext,
        IOrderRepository orderRepository)
    {
        _validator = validator;
        _currentUserContext = currentUserContext;
        _orderRepository = orderRepository;
    }

    public async Task<OrderResult> HandleAsync(
        GetOrderByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        _validator.Validate(query);

        var customerId = GetAuthenticatedCustomerId();

        var order =
            await _orderRepository.GetByIdForCustomerAsync(
                query.OrderId,
                customerId,
                cancellationToken);

        if (order is null)
        {
            throw new NotFoundException(
                "The requested order was not found.");
        }

        return OrderResultMapper.Map(order);
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