using FluentAssertions;
using OrderManager.Domain.Strategies;

namespace OrderManager.Tests.Domain;

public class ReformTaxStrategyTests
{
    private readonly ReformTaxStrategy _sut = new();

    /// <summary>
    /// Verifica se o nome da estratégia é "TaxReform".
    /// </summary>
    [Fact]
    public void StrategyName_ShouldReturnTaxReform()
    {
        // Arrange - _sut já configurado

        // Act
        var result = _sut.StrategyName;

        // Assert
        result.Should().Be("TaxReform");
    }

    /// <summary>
    /// Verifica se o cálculo retorna 20% do valor total para diferentes valores.
    /// </summary>
    [Theory]
    [InlineData(100, 20)]
    [InlineData(200, 40)]
    [InlineData(1000, 200)]
    [InlineData(0, 0)]
    public void CalculateTax_ShouldReturn20Percent(decimal totalAmount, decimal expectedTax)
    {
        // Arrange - valores via InlineData

        // Act
        var result = _sut.CalculateTax(totalAmount);

        // Assert
        result.Should().Be(expectedTax);
    }

    /// <summary>
    /// Verifica se o cálculo mantém precisão decimal correta.
    /// </summary>
    [Fact]
    public void CalculateTax_WithDecimalPrecision_ShouldCalculateCorrectly()
    {
        // Arrange
        var totalAmount = 333.33m;

        // Act
        var result = _sut.CalculateTax(totalAmount);

        // Assert
        result.Should().Be(66.666m);
    }
}
