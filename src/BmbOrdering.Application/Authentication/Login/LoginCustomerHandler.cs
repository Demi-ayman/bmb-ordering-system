using BmbOrdering.Application.Abstractions.Authentication;
using BmbOrdering.Application.Abstractions.Authorization;
using BmbOrdering.Application.Abstractions.Persistence;
using BmbOrdering.Application.Common.Exceptions;

namespace BmbOrdering.Application.Authentication.Login;

public sealed class LoginCustomerHandler
{
    private readonly LoginCustomerValidator _validator;
    private readonly ICustomerRepository _customerRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUserRoleProvider _userRoleProvider;

    public LoginCustomerHandler(
        LoginCustomerValidator validator,
        ICustomerRepository customerRepository,
        IPasswordService passwordService,
        IJwtTokenGenerator jwtTokenGenerator,
        IUserRoleProvider userRoleProvider)
    {
        _validator = validator;
        _customerRepository = customerRepository;
        _passwordService = passwordService;
        _jwtTokenGenerator = jwtTokenGenerator;
        _userRoleProvider = userRoleProvider;
    }

    public async Task<LoginCustomerResult> HandleAsync(
        LoginCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        _validator.Validate(command);

        var normalizedEmail =
            command.Email.Trim().ToUpperInvariant();

        var customer =
            await _customerRepository.GetByNormalizedEmailAsync(
                normalizedEmail,
                cancellationToken);

        if (customer is null ||
            !_passwordService.VerifyPassword(
                command.Password,
                customer.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        var roles = _userRoleProvider.GetRoles(customer);

        var accessToken =
            _jwtTokenGenerator.GenerateToken(customer, roles);

        return new LoginCustomerResult(
            customer.Id,
            customer.FullName,
            customer.Email,
            accessToken.Value,
            accessToken.ExpiresAtUtc,
            customer.BannedUntilUtc);
    }
}
