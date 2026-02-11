using AutoMapper;
using FluentAssertions;
using NSubstitute;
using OrderManager.Application.DTOs;
using OrderManager.Application.Interfaces;
using OrderManager.Application.Mappings;
using OrderManager.Application.Orders.Commands.CreateOrder;
using OrderManager.Domain.Common;
using OrderManager.Domain.Entities;
using OrderManager.Domain.Interfaces;
using OrderManager.Domain.Strategies;

namespace OrderManager.Tests.Application;

public class CreateOrderCommandHandlerTests
{
    private readonly IOrderRepository _orderRepository;
    private readonly ITaxStrategyFactory _taxStrategyFactory;
    private readonly IMapper _mapper;
    private readonly CreateOrderCommandHandler _sut;

    public CreateOrderCommandHandlerTests()
    {
        _orderRepository = Substitute.For<IOrderRepository>();
        _taxStrategyFactory = Substitute.For<ITaxStrategyFactory>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<OrderProfile>());
        _mapper = config.CreateMapper();

        _sut = new CreateOrderCommandHandler(_orderRepository, _taxStrategyFactory, _mapper);
    }

    /// <summary>
    /// Verifica se um pedido válido é criado com sucesso e persiste no repositório.
    /// </summary>
    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        var command = new CreateOrderCommand(
            "PED-001",
            new List<OrderItemRequest> { new("Product A", 2, 50.00m) });

        _orderRepository.ExistsAsync(command.PedidoId).Returns(false);
        _taxStrategyFactory.GetStrategy().Returns(new CurrentTaxStrategy());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PedidoId.Should().Be("PED-001");
        result.Value.TotalAmount.Should().Be(100.00m);
        result.Value.TaxAmount.Should().Be(30.00m);
        result.Value.Status.Should().Be("Processed");

        await _orderRepository.Received(1).AddAsync(Arg.Any<Order>());
    }

    /// <summary>
    /// Verifica se a estratégia de reforma tributária (20%) é aplicada corretamente.
    /// </summary>
    [Fact]
    public async Task Handle_WithNewTaxEnabled_ShouldUse20PercentTax()
    {
        // Arrange
        var command = new CreateOrderCommand(
            "PED-002",
            new List<OrderItemRequest> { new("Product B", 1, 100.00m) });

        _orderRepository.ExistsAsync(command.PedidoId).Returns(false);
        _taxStrategyFactory.GetStrategy().Returns(new ReformTaxStrategy());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TaxAmount.Should().Be(20.00m);
    }

    /// <summary>
    /// Verifica se pedidos duplicados são rejeitados com erro apropriado.
    /// </summary>
    [Fact]
    public async Task Handle_WithDuplicatePedidoId_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new CreateOrderCommand(
            "PED-DUPLICATE",
            new List<OrderItemRequest> { new("Product", 1, 10.00m) });

        _orderRepository.ExistsAsync(command.PedidoId).Returns(true);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Order.Duplicate");
        result.Error.Message.Should().Contain("PED-DUPLICATE");
    }
}
