using AutoMapper;
using FluentAssertions;
using NSubstitute;
using OrderManager.Application.Mappings;
using OrderManager.Application.Orders.Queries.GetOrderById;
using OrderManager.Domain.Entities;
using OrderManager.Domain.Interfaces;

namespace OrderManager.Tests.Application;

public class GetOrderByIdQueryHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly GetOrderByIdQueryHandler _sut;

    public GetOrderByIdQueryHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<OrderProfile>());
        _mapper = config.CreateMapper();

        _sut = new GetOrderByIdQueryHandler(_orderRepository, _mapper);
    }

    [Fact]
    public async Task Handle_WithExistingOrder_ShouldReturnSuccessResult()
    {
        var orderId = Guid.NewGuid();
        var items = new List<OrderItem> { OrderItem.Create("Test", 1, 100m) };
        var order = Order.Create("PED-123", items);

        _orderRepository.GetByIdAsync(orderId).Returns(order);

        var result = await _sut.Handle(new GetOrderByIdQuery(orderId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PedidoId.Should().Be("PED-123");
    }

    [Fact]
    public async Task Handle_WithNonExistingOrder_ShouldReturnFailureResult()
    {
        var orderId = Guid.NewGuid();
        _orderRepository.GetByIdAsync(orderId).Returns((Order?)null);

        var result = await _sut.Handle(new GetOrderByIdQuery(orderId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.NotFound");
    }
}
