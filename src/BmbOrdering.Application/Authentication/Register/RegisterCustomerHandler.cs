using BmbOrdering.Application.Abstractions.Authentication;
using BmbOrdering.Application.Abstractions.Persistence;
using BmbOrdering.Application.Abstractions.Time;
using BmbOrdering.Application.Common.Exceptions;
using BmbOrdering.Domain.Customers;

namespace BmbOrdering.Application.Authentication.Register;

public sealed class RegisterCustomerHandler
{
    private readonly RegisterCustomerValidator _validator;
    private readonly ICustomerRepository _customerRepository;
    private readonly IPasswordService _passwordService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public RegisterCustomerHandler(
        RegisterCustomerValidator validator,
        ICustomerRepository customerRepository,
        IPasswordService passwordService,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _validator = validator;
        _customerRepository = customerRepository;
        _passwordService = passwordService;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<RegisterCustomerResult> HandleAsync(
        RegisterCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        _validator.Validate(command);

        var email = command.Email.Trim();
        var normalizedEmail = email.ToUpperInvariant();

        var emailExists =
            await _customerRepository.ExistsByNormalizedEmailAsync(
                normalizedEmail,
                cancellationToken);

        if (emailExists)
        {
            throw new ConflictException(
                "A customer with this email address already exists.");
        }

        var passwordHash =
            _passwordService.HashPassword(command.Password);

        var customer = Customer.Register(
            command.FullName,
            email,
            normalizedEmail,
            passwordHash,
            _clock.UtcNow);

        _customerRepository.Add(customer);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterCustomerResult(
            customer.Id,
            customer.FullName,
            customer.Email,
            customer.CreatedAtUtc);
    }
}