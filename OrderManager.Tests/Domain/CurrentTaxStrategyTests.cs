using FluentAssertions;
using OrderManager.Domain.Strategies;

namespace OrderManager.Tests.Domain;

public class CurrentTaxStrategyTests
{
    private readonly CurrentTaxStrategy _sut = new();

    /// <summary>
    /// Verifica se o nome da estratégia é "Current".
    /// </summary>
    [Fact]
    public void StrategyName_ShouldReturnCurrent()
    {
        // Arrange - _sut já configurado

        // Act
        var result = _sut.StrategyName;

        // Assert
        result.Should().Be("Current");
    }

    /// <summary>
    /// Verifica se o cálculo retorna 30% do valor total para diferentes valores.
    /// </summary>
    [Theory]
    [InlineData(100, 30)]
    [InlineData(200, 60)]
    [InlineData(1000, 300)]
    [InlineData(0, 0)]
    public void CalculateTax_ShouldReturn30Percent(decimal totalAmount, decimal expectedTax)
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
        result.Should().Be(99.999m);
    }
}
