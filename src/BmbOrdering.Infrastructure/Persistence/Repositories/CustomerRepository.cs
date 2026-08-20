using BmbOrdering.Application.Abstractions.Persistence;
using BmbOrdering.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace BmbOrdering.Infrastructure.Persistence.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly OrderingDbContext _dbContext;

    public CustomerRepository(
        OrderingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsByNormalizedEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Customers.AnyAsync(
            customer =>
                customer.NormalizedEmail == normalizedEmail,
            cancellationToken);
    }

    public Task<Customer?> GetByNormalizedEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Customers.SingleOrDefaultAsync(
            customer =>
                customer.NormalizedEmail == normalizedEmail,
            cancellationToken);
    }

    public Task<Customer?> GetByIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Customers.SingleOrDefaultAsync(
            customer => customer.Id == customerId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var customers = await _dbContext.Customers
            .AsNoTracking()
            .OrderBy(customer => customer.FullName)
            .ThenBy(customer => customer.Email)
            .ToListAsync(cancellationToken);

        return customers;
    }

    public void Add(Customer customer)
    {
        _dbContext.Customers.Add(customer);
    }
}
