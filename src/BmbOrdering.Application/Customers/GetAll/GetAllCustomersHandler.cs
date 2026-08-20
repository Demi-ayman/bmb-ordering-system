using BmbOrdering.Application.Abstractions.Persistence;
using BmbOrdering.Application.Abstractions.Time;

namespace BmbOrdering.Application.Customers.GetAll;

public sealed class GetAllCustomersHandler
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IClock _clock;

    public GetAllCustomersHandler(
        ICustomerRepository customerRepository,
        IClock clock)
    {
        _customerRepository = customerRepository;
        _clock = clock;
    }

    public async Task<IReadOnlyList<CustomerSummaryResult>>
        HandleAsync(
            CancellationToken cancellationToken = default)
    {
        var customers = await _customerRepository.GetAllAsync(
            cancellationToken);

        var utcNow = _clock.UtcNow;

        return customers
            .Select(customer => new CustomerSummaryResult(
                customer.Id,
                customer.FullName,
                customer.Email,
                customer.CreatedAtUtc,
                customer.BannedUntilUtc,
                customer.IsOrderingBannedAt(utcNow)))
            .ToArray();
    }
}
