using Microsoft.Extensions.DependencyInjection;
using OrderManager.Domain.Interfaces;
using OrderManager.Infrastructure.Data.Repositories;

namespace OrderManager.Infrastructure.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddDataInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();

        return services;
    }
}
