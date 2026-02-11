namespace OrderManager.Domain.Strategies;

public interface ITaxCalculationStrategy
{
    decimal CalculateTax(decimal totalAmount);
    string StrategyName { get; }
}
