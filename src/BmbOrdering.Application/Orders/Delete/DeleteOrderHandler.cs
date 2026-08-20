using BmbOrdering.Application.Abstractions.Authentication;
using BmbOrdering.Application.Abstractions.Persistence;
using BmbOrdering.Application.Abstractions.Time;
using BmbOrdering.Application.Common.Exceptions;
using BmbOrdering.Domain.Orders;

namespace BmbOrdering.Application.Orders.Delete;

public sealed class DeleteOrderHandler
{
    private const int DeletionThreshold = 3;

    private readonly DeleteOrderValidator _validator;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly ICustomerRepository _customerRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderDeletionEventRepository
        _deletionEventRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITransactionManager _transactionManager;
    private readonly IClock _clock;

    public DeleteOrderHandler(
        DeleteOrderValidator validator,
        ICurrentUserContext currentUserContext,
        ICustomerRepository customerRepository,
        IOrderRepository orderRepository,
        IOrderDeletionEventRepository deletionEventRepository,
        IUnitOfWork unitOfWork,
        ITransactionManager transactionManager,
        IClock clock)
    {
        _validator = validator;
        _currentUserContext = currentUserContext;
        _customerRepository = customerRepository;
        _orderRepository = orderRepository;
        _deletionEventRepository = deletionEventRepository;
        _unitOfWork = unitOfWork;
        _transactionManager = transactionManager;
        _clock = clock;
    }

    public Task<DeleteOrderResult> HandleAsync(
        DeleteOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        _validator.Validate(command);

        var customerId = GetAuthenticatedCustomerId();

        return _transactionManager.ExecuteSerializableAsync(
            transactionCancellationToken =>
                DeleteWithinTransactionAsync(
                    command.OrderId,
                    customerId,
                    transactionCancellationToken),
            cancellationToken);
    }

    private async Task<DeleteOrderResult>
        DeleteWithinTransactionAsync(
            Guid orderId,
            Guid customerId,
            CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(
            customerId,
            cancellationToken);

        if (customer is null)
        {
            throw new NotFoundException(
                "The authenticated customer was not found.");
        }

        var order =
            await _orderRepository.GetByIdForCustomerAsync(
                orderId,
                customerId,
                cancellationToken);

        if (order is null)
        {
            throw new NotFoundException(
                "The requested order was not found.");
        }

        var deletionAlreadyExists =
            await _deletionEventRepository.ExistsForOrderAsync(
                order.Id,
                cancellationToken);

        if (deletionAlreadyExists)
        {
            throw new ConflictException(
                "The order has already been deleted.");
        }

        var utcNow = _clock.UtcNow;

        order.Delete(utcNow);

        var deletionEvent =
            OrderDeletionEvent.Record(order);

        _deletionEventRepository.Add(deletionEvent);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        var dayStartUtc = new DateTime(
            utcNow.Year,
            utcNow.Month,
            utcNow.Day,
            0,
            0,
            0,
            DateTimeKind.Utc);

        var dayEndUtc = dayStartUtc.AddDays(1);

        var qualifyingDeletionCount =
            await _deletionEventRepository
                .CountQualifyingDeletionsAsync(
                    customerId,
                    dayStartUtc,
                    dayEndUtc,
                    cancellationToken);

        if (deletionEvent.QualifiesForBanCount &&
            qualifyingDeletionCount >= DeletionThreshold)
        {
            customer.ApplyOrderingBan(utcNow);

            await _unitOfWork.SaveChangesAsync(
                cancellationToken);
        }

        return new DeleteOrderResult(
            order.Id,
            deletionEvent.DeletedAtUtc,
            deletionEvent.QualifiesForBanCount,
            qualifyingDeletionCount,
            customer.BannedUntilUtc);
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