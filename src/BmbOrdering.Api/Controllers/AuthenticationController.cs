using BmbOrdering.Api.Contracts.Authentication;
using BmbOrdering.Application.Authentication.Login;
using BmbOrdering.Application.Authentication.Register;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BmbOrdering.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/auth")]
public sealed class AuthenticationController : ControllerBase
{
    private readonly RegisterCustomerHandler
        _registerCustomerHandler;

    private readonly LoginCustomerHandler
        _loginCustomerHandler;

    public AuthenticationController(
        RegisterCustomerHandler registerCustomerHandler,
        LoginCustomerHandler loginCustomerHandler)
    {
        _registerCustomerHandler =
            registerCustomerHandler;

        _loginCustomerHandler =
            loginCustomerHandler;
    }

    [HttpPost("register")]
    [ProducesResponseType(
        typeof(RegisteredCustomerResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisteredCustomerResponse>>
        RegisterAsync(
            RegisterCustomerRequest request,
            CancellationToken cancellationToken)
    {
        var command = new RegisterCustomerCommand(
            request.FullName,
            request.Email,
            request.Password,
            request.PasswordConfirmation);

        var result =
            await _registerCustomerHandler.HandleAsync(
                command,
                cancellationToken);

        var response = new RegisteredCustomerResponse(
            result.CustomerId,
            result.FullName,
            result.Email,
            result.CreatedAtUtc);

        return Created(
            $"/api/v1/customers/{result.CustomerId}",
            response);
    }

    [HttpPost("login")]
    [ProducesResponseType(
        typeof(LoginCustomerResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginCustomerResponse>>
        LoginAsync(
            LoginCustomerRequest request,
            CancellationToken cancellationToken)
    {
        var command = new LoginCustomerCommand(
            request.Email,
            request.Password);

        var result =
            await _loginCustomerHandler.HandleAsync(
                command,
                cancellationToken);

        var response = new LoginCustomerResponse(
            result.CustomerId,
            result.FullName,
            result.Email,
            result.AccessToken,
            result.AccessTokenExpiresAtUtc,
            result.BannedUntilUtc);

        return Ok(response);
    }
}