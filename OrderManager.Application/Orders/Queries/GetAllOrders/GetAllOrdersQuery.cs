using MediatR;
using OrderManager.Application.DTOs;

namespace OrderManager.Application.Orders.Queries.GetAllOrders;

public record GetAllOrdersQuery : IRequest<IEnumerable<OrderResponse>>;
