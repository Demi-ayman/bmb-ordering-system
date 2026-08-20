using BmbOrdering.Application.Abstractions.Authentication;
using BmbOrdering.Application.Abstractions.Persistence;
using BmbOrdering.Application.Abstractions.Time;
using BmbOrdering.Application.Common.Exceptions;
using BmbOrdering.Application.Orders.Common;
using BmbOrdering.Domain.Orders;

namespace BmbOrdering.Application.Orders.Create;

public sealed class CreateOrderHandler
{
    private readonly CreateOrderValidator _validator;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ICustomerRepository _customerRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public CreateOrderHandler(
        CreateOrderValidator validator,
        ICurrentUserContext currentUserContext,
        ICustomerRepository customerRepository,
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _validator = validator;
        _currentUserContext = currentUserContext;
        _customerRepository = customerRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<OrderResult> HandleAsync(
        CreateOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        _validator.Validate(command);

        var customerId = GetAuthenticatedCustomerId();
        var utcNow = _clock.UtcNow;

        var customer = await _customerRepository.GetByIdAsync(
            customerId,
            cancellationToken);

        if (customer is null)
        {
            throw new NotFoundException(
                "The authenticated customer was not found.");
        }

        if (customer.IsOrderingBannedAt(utcNow))
        {
            throw new OrderingBannedException(
                customer.BannedUntilUtc!.Value);
        }

        var itemDetails = command.Items
            .Select(item => new OrderItemDetails(
                item.ProductName,
                item.Quantity,
                item.UnitPrice))
            .ToArray();

        var order = Order.Create(
            customerId,
            GenerateOrderNumber(utcNow),
            itemDetails,
            utcNow);

        _orderRepository.Add(order);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

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

    private static string GenerateOrderNumber(DateTime utcNow)
    {
        var suffix = Guid.NewGuid()
            .ToString("N")[..8]
            .ToUpperInvariant();

        return $"ORD-{utcNow:yyyyMMddHHmmss}-{suffix}";
    }
}