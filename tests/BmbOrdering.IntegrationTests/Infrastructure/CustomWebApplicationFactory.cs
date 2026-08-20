using BmbOrdering.Application.Abstractions.Time;
using BmbOrdering.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BmbOrdering.IntegrationTests.Infrastructure;

public sealed class CustomWebApplicationFactory :
    WebApplicationFactory<Program>,
    IAsyncLifetime
{
    public const string AdministratorEmail =
        "integration.admin@example.com";

    private const string SigningKey =
        "integration-tests-signing-key-contains-at-least-32-bytes";

    private readonly string _databaseName =
        $"BmbOrderingIntegrationTests_{Guid.NewGuid():N}";

    public FixedClock Clock { get; } = new(DateTime.UtcNow);

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting(
            "ConnectionStrings:OrderingDatabase",
            $"Server=(localdb)\\MSSQLLocalDB;" +
            $"Database={_databaseName};" +
            "Trusted_Connection=True;" +
            "TrustServerCertificate=True");
        builder.UseSetting(
            "Jwt:Issuer",
            "BmbOrdering.IntegrationTests");
        builder.UseSetting(
            "Jwt:Audience",
            "BmbOrdering.IntegrationTests.Client");
        builder.UseSetting("Jwt:SigningKey", SigningKey);
        builder.UseSetting("Jwt:ExpirationMinutes", "30");
        builder.UseSetting(
            "Authorization:AdministratorEmails:0",
            AdministratorEmail);
        builder.UseSetting("Logging:LogLevel:Default", "Warning");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IClock>();
            services.AddSingleton<IClock>(Clock);
        });
    }

    public HttpClient CreateHttpsClient()
    {
        return CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost"),
                AllowAutoRedirect = false
            });
    }

    public async Task InitializeAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<OrderingDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<OrderingDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
        Dispose();
    }
}
