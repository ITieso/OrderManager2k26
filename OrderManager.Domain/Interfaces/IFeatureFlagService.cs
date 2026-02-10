namespace OrderManager.Domain.Interfaces;

public interface IFeatureFlagService
{
    bool IsNewTaxCalculationEnabled();
}
