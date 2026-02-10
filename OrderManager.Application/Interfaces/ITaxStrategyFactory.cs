using OrderManager.Domain.Strategies;

namespace OrderManager.Application.Interfaces;

public interface ITaxStrategyFactory
{
    ITaxCalculationStrategy GetStrategy();
}
