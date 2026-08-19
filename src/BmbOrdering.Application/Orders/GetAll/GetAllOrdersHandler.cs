using BmbOrdering.Application.Abstractions.Persistence;
using BmbOrdering.Application.Orders.Common;

namespace BmbOrdering.Application.Orders.GetAll;

public sealed class GetAllOrdersHandler
{
    private readonly IOrderRepository _orderRepository;

    public GetAllOrdersHandler(
        IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<IReadOnlyList<OrderResult>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var orders = await _orderRepository.GetAllAsync(
            cancellationToken);

        return orders
            .Select(OrderResultMapper.Map)
            .ToArray();
    }
}