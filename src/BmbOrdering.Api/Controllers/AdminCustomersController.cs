using BmbOrdering.Api.Contracts.Customers;
using BmbOrdering.Api.Contracts.Orders;
using BmbOrdering.Api.Mappings;
using BmbOrdering.Application.Common.Authorization;
using BmbOrdering.Application.Customers.GetAll;
using BmbOrdering.Application.Customers.GetOrders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BmbOrdering.Api.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.Administrator)]
[Route("api/v1/admin/customers")]
public sealed class AdminCustomersController : ControllerBase
{
    private readonly GetAllCustomersHandler
        _getAllCustomersHandler;
    private readonly GetCustomerOrdersForAdminHandler
        _getCustomerOrdersHandler;

    public AdminCustomersController(
        GetAllCustomersHandler getAllCustomersHandler,
        GetCustomerOrdersForAdminHandler getCustomerOrdersHandler)
    {
        _getAllCustomersHandler = getAllCustomersHandler;
        _getCustomerOrdersHandler = getCustomerOrdersHandler;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(CustomerSummaryResponse[]),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status403Forbidden)]
    public async Task<
        ActionResult<IReadOnlyCollection<CustomerSummaryResponse>>>
        GetAllAsync(CancellationToken cancellationToken)
    {
        var results = await _getAllCustomersHandler.HandleAsync(
            cancellationToken);

        var response = results
            .Select(customer => new CustomerSummaryResponse(
                customer.Id,
                customer.FullName,
                customer.Email,
                customer.CreatedAtUtc,
                customer.BannedUntilUtc,
                customer.IsOrderingBanned))
            .ToArray();

        return Ok(response);
    }

    [HttpGet("{customerId:guid}/orders")]
    [ProducesResponseType(
        typeof(OrderResponse[]),
        StatusCodes.Status200OK)]
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
    public async Task<
        ActionResult<IReadOnlyCollection<OrderResponse>>>
        GetOrdersAsync(
            Guid customerId,
            CancellationToken cancellationToken)
    {
        var query = new GetCustomerOrdersForAdminQuery(
            customerId);

        var results = await _getCustomerOrdersHandler.HandleAsync(
            query,
            cancellationToken);

        var response = results
            .Select(OrderResponseMapper.Map)
            .ToArray();

        return Ok(response);
    }
}
