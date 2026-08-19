using BmbOrdering.Application.Abstractions.Authentication;
using BmbOrdering.Application.Abstractions.Persistence;
using BmbOrdering.Application.Abstractions.Time;
using BmbOrdering.Infrastructure.Authentication;
using BmbOrdering.Infrastructure.Persistence;
using BmbOrdering.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BmbOrdering.Infrastructure.Persistence.Repositories;

namespace BmbOrdering.Infrastructure;

public static class DependencyInjection
{
    private const string ConnectionStringName =
        "OrderingDatabase";

    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString(
                ConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is not configured.");
        }

        services.AddDbContext<OrderingDbContext>(
            options =>
                options.UseSqlServer(
                    connectionString,
                    sqlServerOptions =>
                    {
                        sqlServerOptions.MigrationsAssembly(
                            typeof(OrderingDbContext)
                                .Assembly
                                .FullName);

                        sqlServerOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay:
                                TimeSpan.FromSeconds(5),
                            errorNumbersToAdd: null);
                    }));
        services.AddScoped<
                ICustomerRepository,
                CustomerRepository>();
        services.AddScoped<
                IOrderRepository,
                OrderRepository>();
        services.AddScoped<
                IOrderDeletionEventRepository,
                OrderDeletionEventRepository>();

        services.AddScoped<IUnitOfWork>(
            serviceProvider =>
                serviceProvider.GetRequiredService<
                    OrderingDbContext>());
        services.AddScoped<
                ITransactionManager,
                TransactionManager>();

        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}