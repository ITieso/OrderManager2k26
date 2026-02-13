using AutoMapper;
using OrderManager.Application.Mappings;

namespace OrderManager.Tests.Application;

public class OrderProfileTests
{
    /// <summary>
    /// Verifica se a configuração do AutoMapper está válida e sem erros.
    /// </summary>
    [Fact]
    public void AutoMapper_Configuration_ShouldBeValid()
    {
        // Arrange
        var config = new MapperConfiguration(cfg => cfg.AddProfile<OrderProfile>());

        // Act & Assert
        config.AssertConfigurationIsValid();
    }
}
