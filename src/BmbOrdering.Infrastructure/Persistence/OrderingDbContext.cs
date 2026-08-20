using BmbOrdering.Application.Abstractions.Persistence;
using BmbOrdering.Domain.Customers;
using BmbOrdering.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace BmbOrdering.Infrastructure.Persistence;

public sealed class OrderingDbContext : DbContext, IUnitOfWork
{
    public OrderingDbContext(
        DbContextOptions<OrderingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers =>
        Set<Customer>();

    public DbSet<Order> Orders =>
        Set<Order>();

    public DbSet<OrderItem> OrderItems =>
        Set<OrderItem>();

    public DbSet<OrderDeletionEvent> OrderDeletionEvents =>
        Set<OrderDeletionEvent>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(OrderingDbContext).Assembly);
    }
}