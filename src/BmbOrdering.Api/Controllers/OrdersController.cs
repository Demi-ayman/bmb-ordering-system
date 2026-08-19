using BmbOrdering.Api.Contracts.Orders;
using BmbOrdering.Application.Common.Authorization;
using BmbOrdering.Application.Orders.Common;
using BmbOrdering.Application.Orders.Create;
using BmbOrdering.Application.Orders.Delete;
using BmbOrdering.Application.Orders.GetById;
using BmbOrdering.Application.Orders.GetForCurrentCustomer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BmbOrdering.Api.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Customer)]
[Route("api/v1/orders")]
public sealed class OrdersController : ControllerBase
{
	private readonly CreateOrderHandler _createOrderHandler;
	private readonly GetOrderByIdHandler _getOrderByIdHandler;
	private readonly GetCustomerOrdersHandler
		_getCustomerOrdersHandler;
	private readonly DeleteOrderHandler _deleteOrderHandler;

	public OrdersController(
		CreateOrderHandler createOrderHandler,
		GetOrderByIdHandler getOrderByIdHandler,
		GetCustomerOrdersHandler getCustomerOrdersHandler,
		DeleteOrderHandler deleteOrderHandler)
	{
		_createOrderHandler = createOrderHandler;
		_getOrderByIdHandler = getOrderByIdHandler;
		_getCustomerOrdersHandler = getCustomerOrdersHandler;
		_deleteOrderHandler = deleteOrderHandler;
	}

	[HttpPost]
	[ProducesResponseType(
		typeof(OrderResponse),
		StatusCodes.Status201Created)]
	[ProducesResponseType(
		typeof(ValidationProblemDetails),
		StatusCodes.Status400BadRequest)]
	[ProducesResponseType(
		typeof(ProblemDetails),
		StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(
		typeof(ProblemDetails),
		StatusCodes.Status403Forbidden)]
	[ProducesResponseType(
		typeof(ProblemDetails),
		StatusCodes.Status404NotFound)]
	public async Task<ActionResult<OrderResponse>> CreateAsync(
		CreateOrderRequest request,
		CancellationToken cancellationToken)
	{
		var items = request.Items?
			.Select(item => new CreateOrderItemCommand(
				item.ProductName,
				item.Quantity,
				item.UnitPrice))
			.ToArray()
			?? Array.Empty<CreateOrderItemCommand>();

		var command = new CreateOrderCommand(items);

		var result = await _createOrderHandler.HandleAsync(
			command,
			cancellationToken);

		return Created(
			$"/api/v1/orders/{result.Id}",
			MapResponse(result));
	}

	[HttpGet]
	[ProducesResponseType(
		typeof(OrderResponse[]),
		StatusCodes.Status200OK)]
	[ProducesResponseType(
		typeof(ProblemDetails),
		StatusCodes.Status401Unauthorized)]
	public async Task<
		ActionResult<IReadOnlyCollection<OrderResponse>>>
		GetCurrentCustomerOrdersAsync(
			CancellationToken cancellationToken)
	{
		var results =
			await _getCustomerOrdersHandler.HandleAsync(
				cancellationToken);

		var response = results
			.Select(MapResponse)
			.ToArray();

		return Ok(response);
	}

	[HttpGet("{orderId:guid}")]
	[ProducesResponseType(
		typeof(OrderResponse),
		StatusCodes.Status200OK)]
	[ProducesResponseType(
		typeof(ValidationProblemDetails),
		StatusCodes.Status400BadRequest)]
	[ProducesResponseType(
		typeof(ProblemDetails),
		StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(
		typeof(ProblemDetails),
		StatusCodes.Status404NotFound)]
	public async Task<ActionResult<OrderResponse>> GetByIdAsync(
		Guid orderId,
		CancellationToken cancellationToken)
	{
		var query = new GetOrderByIdQuery(orderId);

		var result = await _getOrderByIdHandler.HandleAsync(
			query,
			cancellationToken);

		return Ok(MapResponse(result));
	}

	[HttpDelete("{orderId:guid}")]
	[ProducesResponseType(
		typeof(DeleteOrderResponse),
		StatusCodes.Status200OK)]
	[ProducesResponseType(
		typeof(ValidationProblemDetails),
		StatusCodes.Status400BadRequest)]
	[ProducesResponseType(
		typeof(ProblemDetails),
		StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(
		typeof(ProblemDetails),
		StatusCodes.Status404NotFound)]
	[ProducesResponseType(
		typeof(ProblemDetails),
		StatusCodes.Status409Conflict)]
	public async Task<ActionResult<DeleteOrderResponse>> DeleteAsync(
		Guid orderId,
		CancellationToken cancellationToken)
	{
		var command = new DeleteOrderCommand(orderId);

		var result = await _deleteOrderHandler.HandleAsync(
			command,
			cancellationToken);

		var response = new DeleteOrderResponse(
			result.OrderId,
			result.DeletedAtUtc,
			result.QualifiesForBanCount,
			result.QualifyingDeletionCount,
			result.BannedUntilUtc);

		return Ok(response);
	}

	private static OrderResponse MapResponse(
		OrderResult result)
	{
		var items = result.Items
			.Select(item => new OrderItemResponse(
				item.Id,
				item.ProductName,
				item.Quantity,
				item.UnitPrice,
				item.LineTotal))
			.ToArray();

		return new OrderResponse(
			result.Id,
			result.CustomerId,
			result.OrderNumber,
			result.Status,
			result.TotalAmount,
			result.CreatedAtUtc,
			result.DeletedAtUtc,
			items);
	}
}