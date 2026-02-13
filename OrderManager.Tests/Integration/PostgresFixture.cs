using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManager.Domain.Interfaces;
using OrderManager.Infrastructure.Data;
using OrderManager.Infrastructure.Data.Repositories;
using Testcontainers.PostgreSql;

namespace OrderManager.Tests.Integration;

/// <summary>
/// Fixture que configura um container PostgreSQL para testes de integração.
/// O container é iniciado automaticamente e destruído após os testes.
/// Requer Docker Desktop rodando.
/// </summary>
public class PostgresFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("ordermanager_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public string ConnectionString => _postgres.GetConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove registros existentes do DbContext e IOrderRepository
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<OrderDbContext>));
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            var repositoryDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IOrderRepository));
            if (repositoryDescriptor != null)
                services.Remove(repositoryDescriptor);

            // Adiciona DbContext com PostgreSQL do container
            services.AddDbContext<OrderDbContext>(options =>
                options.UseNpgsql(ConnectionString));

            // Adiciona repositório EF
            services.AddScoped<IOrderRepository, EfOrderRepository>();
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // Cria o schema do banco
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        await context.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}
