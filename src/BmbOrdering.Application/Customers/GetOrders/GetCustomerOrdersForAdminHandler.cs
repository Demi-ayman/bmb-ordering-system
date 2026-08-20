using BmbOrdering.Application.Abstractions.Persistence;
using BmbOrdering.Application.Common.Exceptions;
using BmbOrdering.Application.Orders.Common;

namespace BmbOrdering.Application.Customers.GetOrders;

public sealed class GetCustomerOrdersForAdminHandler
{
    private readonly GetCustomerOrdersForAdminValidator _validator;
    private readonly ICustomerRepository _customerRepository;
    private readonly IOrderRepository _orderRepository;

    public GetCustomerOrdersForAdminHandler(
        GetCustomerOrdersForAdminValidator validator,
        ICustomerRepository customerRepository,
        IOrderRepository orderRepository)
    {
        _validator = validator;
        _customerRepository = customerRepository;
        _orderRepository = orderRepository;
    }

    public async Task<IReadOnlyList<OrderResult>> HandleAsync(
        GetCustomerOrdersForAdminQuery query,
        CancellationToken cancellationToken = default)
    {
        _validator.Validate(query);

        var customer = await _customerRepository.GetByIdAsync(
            query.CustomerId,
            cancellationToken);

        if (customer is null)
        {
            throw new NotFoundException(
                "The requested customer was not found.");
        }

        var orders = await _orderRepository
            .GetAllByCustomerIdAsync(
                query.CustomerId,
                cancellationToken);

        return orders
            .Select(OrderResultMapper.Map)
            .ToArray();
    }
}
