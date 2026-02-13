using FluentAssertions;
using NSubstitute;
using OrderManager.Application.Interfaces;
using OrderManager.Application.Services;
using OrderManager.Domain.Interfaces;
using OrderManager.Domain.Strategies;

namespace OrderManager.Tests.Application;

public class TaxStrategyFactoryTests
{
    private readonly IFeatureFlagService _featureFlagService;
    private readonly CurrentTaxStrategy _currentStrategy;
    private readonly ReformTaxStrategy _reformStrategy;
    private readonly TaxStrategyFactory _sut;

    public TaxStrategyFactoryTests()
    {
        _featureFlagService = Substitute.For<IFeatureFlagService>();
        _currentStrategy = new CurrentTaxStrategy();
        _reformStrategy = new ReformTaxStrategy();
        _sut = new TaxStrategyFactory(_featureFlagService, _currentStrategy, _reformStrategy);
    }

    /// <summary>
    /// Verifica se retorna estratégia atual (30%) quando feature flag desabilitada.
    /// </summary>
    [Fact]
    public void GetStrategy_WhenFeatureFlagDisabled_ShouldReturnCurrentStrategy()
    {
        // Arrange
        _featureFlagService.IsNewTaxCalculationEnabled().Returns(false);

        // Act
        var result = _sut.GetStrategy();

        // Assert
        result.Should().BeOfType<CurrentTaxStrategy>();
        result.StrategyName.Should().Be("Current");
    }

    /// <summary>
    /// Verifica se retorna estratégia de reforma (20%) quando feature flag habilitada.
    /// </summary>
    [Fact]
    public void GetStrategy_WhenFeatureFlagEnabled_ShouldReturnReformStrategy()
    {
        // Arrange
        _featureFlagService.IsNewTaxCalculationEnabled().Returns(true);

        // Act
        var result = _sut.GetStrategy();

        // Assert
        result.Should().BeOfType<ReformTaxStrategy>();
        result.StrategyName.Should().Be("TaxReform");
    }
}
