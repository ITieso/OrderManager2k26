using AutoMapper;
using MediatR;
using OrderManager.Application.DTOs;
using OrderManager.Application.Interfaces;
using OrderManager.Domain.Common;
using OrderManager.Domain.Entities;
using OrderManager.Domain.Interfaces;

namespace OrderManager.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<OrderResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ITaxStrategyFactory _taxStrategyFactory;
    private readonly IMapper _mapper;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        ITaxStrategyFactory taxStrategyFactory,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _taxStrategyFactory = taxStrategyFactory;
        _mapper = mapper;
    }

    public async Task<Result<OrderResponse>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if (await _orderRepository.ExistsAsync(request.PedidoId))
        {
            return Result.Failure<OrderResponse>(OrderErrors.Duplicate(request.PedidoId));
        }

        var items = request.Items
            .Select(i => OrderItem.Create(i.ProductName, i.Quantity, i.UnitPrice))
            .ToList();

        var order = Order.Create(request.PedidoId, items);
        order.MarkAsProcessing();

        var taxStrategy = _taxStrategyFactory.GetStrategy();
        var taxAmount = taxStrategy.CalculateTax(order.TotalAmount);
        order.ApplyTax(taxAmount);

        order.MarkAsProcessed();

        await _orderRepository.AddAsync(order);

        return _mapper.Map<OrderResponse>(order);
    }
}
