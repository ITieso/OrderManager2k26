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

    /// <summary>
    /// Verifica se um pedido existente é retornado com sucesso.
    /// </summary>
    [Fact]
    public async Task Handle_WithExistingOrder_ShouldReturnSuccessResult()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var items = new List<OrderItem> { OrderItem.Create("Test", 1, 100m) };
        var order = Order.Create("PED-123", items);

        _orderRepository.GetByIdAsync(orderId).Returns(order);

        // Act
        var result = await _sut.Handle(new GetOrderByIdQuery(orderId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PedidoId.Should().Be("PED-123");
    }

    /// <summary>
    /// Verifica se pedido inexistente retorna erro NotFound.
    /// </summary>
    [Fact]
    public async Task Handle_WithNonExistingOrder_ShouldReturnFailureResult()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderRepository.GetByIdAsync(orderId).Returns((Order?)null);

        // Act
        var result = await _sut.Handle(new GetOrderByIdQuery(orderId), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.NotFound");
    }
}
