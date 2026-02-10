using Microsoft.Extensions.Configuration;
using OrderManager.Domain.Interfaces;

namespace OrderManager.Infrastructure.ExternalServices.Services;

public class FeatureFlagService : IFeatureFlagService
{
    private readonly IConfiguration _configuration;

    public FeatureFlagService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool IsNewTaxCalculationEnabled()
    {
        return _configuration.GetValue<bool>("FeatureFlags:UseNewTaxCalculation");
    }
}
