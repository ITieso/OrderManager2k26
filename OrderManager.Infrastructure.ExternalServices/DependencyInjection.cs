using Microsoft.Extensions.DependencyInjection;
using OrderManager.Domain.Interfaces;
using OrderManager.Infrastructure.ExternalServices.Services;

namespace OrderManager.Infrastructure.ExternalServices;

public static class DependencyInjection
{
    public static IServiceCollection AddExternalServices(this IServiceCollection services)
    {
        services.AddScoped<IFeatureFlagService, FeatureFlagService>();

        return services;
    }
}
