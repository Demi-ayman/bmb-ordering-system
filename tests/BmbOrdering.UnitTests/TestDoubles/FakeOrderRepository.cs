using BmbOrdering.Application.Abstractions.Persistence;
using BmbOrdering.Domain.Orders;

namespace BmbOrdering.UnitTests.TestDoubles;

public sealed class FakeOrderRepository : IOrderRepository
{
	private readonly List<Order> _orders = new();

	public IReadOnlyCollection<Order> Orders =>
		_orders.AsReadOnly();

	public Task<Order?> GetByIdForCustomerAsync(
		Guid orderId,
		Guid customerId,
		CancellationToken cancellationToken = default)
	{
		var order = _orders.SingleOrDefault(
			candidate =>
				candidate.Id == orderId &&
				candidate.CustomerId == customerId &&
				!candidate.IsDeleted);

		return Task.FromResult(order);
	}

	public Task<IReadOnlyList<Order>> GetByCustomerIdAsync(
		Guid customerId,
		CancellationToken cancellationToken = default)
	{
		IReadOnlyList<Order> orders = _orders
			.Where(order =>
				order.CustomerId == customerId &&
				!order.IsDeleted)
			.OrderByDescending(order => order.CreatedAtUtc)
			.ToArray();

		return Task.FromResult(orders);
	}

	public Task<IReadOnlyList<Order>> GetAllAsync(
		CancellationToken cancellationToken = default)
	{
		IReadOnlyList<Order> orders = _orders
			.OrderByDescending(order => order.CreatedAtUtc)
			.ToArray();

		return Task.FromResult(orders);
	}

	public void Add(Order order)
	{
		_orders.Add(order);
	}

	public void Seed(Order order)
	{
		_orders.Add(order);
	}
}