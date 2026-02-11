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

    [Fact]
    public void GetStrategy_WhenFeatureFlagDisabled_ShouldReturnCurrentStrategy()
    {
        _featureFlagService.IsNewTaxCalculationEnabled().Returns(false);

        var result = _sut.GetStrategy();

        result.Should().BeOfType<CurrentTaxStrategy>();
        result.StrategyName.Should().Be("Current");
    }

    [Fact]
    public void GetStrategy_WhenFeatureFlagEnabled_ShouldReturnReformStrategy()
    {
        _featureFlagService.IsNewTaxCalculationEnabled().Returns(true);

        var result = _sut.GetStrategy();

        result.Should().BeOfType<ReformTaxStrategy>();
        result.StrategyName.Should().Be("TaxReform");
    }
}
