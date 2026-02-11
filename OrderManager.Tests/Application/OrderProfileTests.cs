using AutoMapper;
using OrderManager.Application.Mappings;

namespace OrderManager.Tests.Application;

public class OrderProfileTests
{
    [Fact]
    public void AutoMapper_Configuration_ShouldBeValid()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<OrderProfile>());

        config.AssertConfigurationIsValid();
    }
}
