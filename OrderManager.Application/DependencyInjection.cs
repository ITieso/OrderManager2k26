using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OrderManager.Application.Behaviors;
using OrderManager.Application.Interfaces;
using OrderManager.Application.Services;
using OrderManager.Domain.Strategies;

namespace OrderManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // MediatR with Pipeline Behaviors
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        // AutoMapper
        services.AddAutoMapper(assembly);

        // Tax Strategies (Singleton - stateless)
        services.AddSingleton<CurrentTaxStrategy>();
        services.AddSingleton<ReformTaxStrategy>();

        // Tax Strategy Factory (Scoped - depends on Feature Flag per request)
        services.AddScoped<ITaxStrategyFactory, TaxStrategyFactory>();

        // FluentValidation Validators
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
