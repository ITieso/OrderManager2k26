using AutoMapper;
using MediatR;
using OrderManager.Application.DTOs;
using OrderManager.Domain.Common;
using OrderManager.Domain.Interfaces;

namespace OrderManager.Application.Orders.Queries.GetOrderByPedidoId;

public class GetOrderByPedidoIdQueryHandler : IRequestHandler<GetOrderByPedidoIdQuery, Result<OrderResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrderByPedidoIdQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<Result<OrderResponse>> Handle(GetOrderByPedidoIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByPedidoIdAsync(request.PedidoId);

        if (order is null)
        {
            return Result.Failure<OrderResponse>(OrderErrors.NotFoundByPedidoId(request.PedidoId));
        }

        return _mapper.Map<OrderResponse>(order);
    }
}
