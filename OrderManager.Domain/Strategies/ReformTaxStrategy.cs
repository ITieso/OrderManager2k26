namespace OrderManager.Domain.Strategies;

public class ReformTaxStrategy : ITaxCalculationStrategy
{
    private const decimal TaxRate = 0.20m;

    public string StrategyName => "TaxReform";

    public decimal CalculateTax(decimal totalAmount)
    {
        return totalAmount * TaxRate;
    }
}
