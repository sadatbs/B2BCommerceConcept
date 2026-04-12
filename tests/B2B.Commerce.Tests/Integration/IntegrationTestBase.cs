using B2B.Commerce.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace B2B.Commerce.Tests.Integration;

public class IntegrationTestBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("b2b_commerce_test")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .Build();

    protected WebApplicationFactory<Program> Factory { get; private set; } = null!;
    protected HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<CommerceDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<CommerceDbContext>(options =>
                        options.UseNpgsql(_postgres.GetConnectionString())
                               .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
                });

                builder.UseEnvironment("Testing");
            });

        Client = Factory.CreateClient();

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CommerceDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
