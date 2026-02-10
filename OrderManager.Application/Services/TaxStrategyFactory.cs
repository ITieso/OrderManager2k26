using OrderManager.Application.Interfaces;
using OrderManager.Domain.Interfaces;
using OrderManager.Domain.Strategies;

namespace OrderManager.Application.Services;

public class TaxStrategyFactory : ITaxStrategyFactory
{
    private readonly IFeatureFlagService _featureFlagService;
    private readonly ITaxCalculationStrategy _currentStrategy;
    private readonly ITaxCalculationStrategy _reformStrategy;

    public TaxStrategyFactory(
        IFeatureFlagService featureFlagService,
        CurrentTaxStrategy currentStrategy,
        ReformTaxStrategy reformStrategy)
    {
        _featureFlagService = featureFlagService;
        _currentStrategy = currentStrategy;
        _reformStrategy = reformStrategy;
    }

    public ITaxCalculationStrategy GetStrategy()
    {
        return _featureFlagService.IsNewTaxCalculationEnabled()
            ? _reformStrategy
            : _currentStrategy;
    }
}
