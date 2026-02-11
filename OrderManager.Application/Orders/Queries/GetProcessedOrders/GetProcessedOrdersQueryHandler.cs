using AutoMapper;
using MediatR;
using OrderManager.Application.DTOs;
using OrderManager.Domain.Interfaces;

namespace OrderManager.Application.Orders.Queries.GetProcessedOrders;

public class GetProcessedOrdersQueryHandler : IRequestHandler<GetProcessedOrdersQuery, IEnumerable<OrderResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetProcessedOrdersQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<OrderResponse>> Handle(GetProcessedOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetProcessedOrdersAsync();
        return _mapper.Map<IEnumerable<OrderResponse>>(orders);
    }
}
