using AutoMapper;
using MediatR;
using OrderManager.Application.DTOs;
using OrderManager.Domain.Common;
using OrderManager.Domain.Interfaces;

namespace OrderManager.Application.Orders.Queries.GetOrderById;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<Result<OrderResponse>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id);

        if (order is null)
        {
            return Result.Failure<OrderResponse>(OrderErrors.NotFound(request.Id));
        }

        return _mapper.Map<OrderResponse>(order);
    }
}
