using BmbOrdering.Application.Abstractions.Persistence;
using BmbOrdering.Domain.Customers;

namespace BmbOrdering.UnitTests.TestDoubles;

public sealed class FakeCustomerRepository : ICustomerRepository
{
    private readonly List<Customer> _customers = new();

    public IReadOnlyCollection<Customer> Customers =>
        _customers.AsReadOnly();

    public Task<bool> ExistsByNormalizedEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        var exists = _customers.Any(
            customer => string.Equals(
                customer.NormalizedEmail,
                normalizedEmail,
                StringComparison.Ordinal));

        return Task.FromResult(exists);
    }

    public Task<Customer?> GetByNormalizedEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        var customer = _customers.SingleOrDefault(
            candidate => string.Equals(
                candidate.NormalizedEmail,
                normalizedEmail,
                StringComparison.Ordinal));

        return Task.FromResult(customer);
    }

    public Task<Customer?> GetByIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var customer = _customers.SingleOrDefault(
            candidate => candidate.Id == customerId);

        return Task.FromResult(customer);
    }

    public Task<IReadOnlyList<Customer>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Customer> customers = _customers
            .OrderBy(customer => customer.FullName)
            .ThenBy(customer => customer.Email)
            .ToArray();

        return Task.FromResult(customers);
    }

    public void Add(Customer customer)
    {
        _customers.Add(customer);
    }

    public void Seed(Customer customer)
    {
        _customers.Add(customer);
    }
}
