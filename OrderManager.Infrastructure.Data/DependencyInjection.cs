using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManager.Domain.Interfaces;
using OrderManager.Infrastructure.Data.Repositories;

namespace OrderManager.Infrastructure.Data;

public static class DependencyInjection
{
    /// <summary>
    /// Adiciona infraestrutura de dados com repositório in-memory (para desenvolvimento).
    /// </summary>
    public static IServiceCollection AddDataInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
        return services;
    }

    /// <summary>
    /// Adiciona infraestrutura de dados com PostgreSQL via Entity Framework.
    /// </summary>
    public static IServiceCollection AddDataInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<OrderDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IOrderRepository, EfOrderRepository>();
        return services;
    }
}
