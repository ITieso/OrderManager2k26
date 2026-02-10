namespace OrderManager.Domain.Strategies;

public class CurrentTaxStrategy : ITaxCalculationStrategy
{
    private const decimal TaxRate = 0.30m;

    public string StrategyName => "Current";

    public decimal CalculateTax(decimal totalAmount)
    {
        return totalAmount * TaxRate;
    }
}
