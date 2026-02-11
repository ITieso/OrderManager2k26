using FluentAssertions;
using OrderManager.Domain.Strategies;

namespace OrderManager.Tests.Domain;

public class CurrentTaxStrategyTests
{
    private readonly CurrentTaxStrategy _sut = new();

    [Fact]
    public void StrategyName_ShouldReturnCurrent()
    {
        _sut.StrategyName.Should().Be("Current");
    }

    [Theory]
    [InlineData(100, 30)]
    [InlineData(200, 60)]
    [InlineData(1000, 300)]
    [InlineData(0, 0)]
    public void CalculateTax_ShouldReturn30Percent(decimal totalAmount, decimal expectedTax)
    {
        var result = _sut.CalculateTax(totalAmount);

        result.Should().Be(expectedTax);
    }

    [Fact]
    public void CalculateTax_WithDecimalPrecision_ShouldCalculateCorrectly()
    {
        var result = _sut.CalculateTax(333.33m);

        result.Should().Be(99.999m);
    }
}
