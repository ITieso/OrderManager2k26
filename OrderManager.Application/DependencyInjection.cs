using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OrderManager.Application.Interfaces;
using OrderManager.Application.Services;
using OrderManager.Domain.Strategies;

namespace OrderManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // MediatR - escaneia assembly para Handlers
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // AutoMapper - escaneia assembly para Profiles
        services.AddAutoMapper(assembly);

        // Strategies - Singleton pois são stateless
        services.AddSingleton<CurrentTaxStrategy>();
        services.AddSingleton<ReformTaxStrategy>();

        // Factory - Scoped para resolver Feature Flag por request
        services.AddScoped<ITaxStrategyFactory, TaxStrategyFactory>();

        // Validators
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
