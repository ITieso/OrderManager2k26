using MediatR;
using OrderManager.Application.DTOs;

namespace OrderManager.Application.Orders.Queries.GetProcessedOrders;

public record GetProcessedOrdersQuery : IRequest<IEnumerable<OrderResponse>>;
