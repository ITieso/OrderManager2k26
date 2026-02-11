using FluentAssertions;
using OrderManager.Domain.Strategies;

namespace OrderManager.Tests.Domain;

public class ReformTaxStrategyTests
{
    private readonly ReformTaxStrategy _sut = new();

    [Fact]
    public void StrategyName_ShouldReturnTaxReform()
    {
        _sut.StrategyName.Should().Be("TaxReform");
    }

    [Theory]
    [InlineData(100, 20)]
    [InlineData(200, 40)]
    [InlineData(1000, 200)]
    [InlineData(0, 0)]
    public void CalculateTax_ShouldReturn20Percent(decimal totalAmount, decimal expectedTax)
    {
        var result = _sut.CalculateTax(totalAmount);

        result.Should().Be(expectedTax);
    }

    [Fact]
    public void CalculateTax_WithDecimalPrecision_ShouldCalculateCorrectly()
    {
        var result = _sut.CalculateTax(333.33m);

        result.Should().Be(66.666m);
    }
}
