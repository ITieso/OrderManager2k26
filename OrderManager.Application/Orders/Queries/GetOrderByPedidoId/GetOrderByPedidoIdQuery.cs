using MediatR;
using OrderManager.Application.DTOs;
using OrderManager.Domain.Common;

namespace OrderManager.Application.Orders.Queries.GetOrderByPedidoId;

public record GetOrderByPedidoIdQuery(string PedidoId) : IRequest<Result<OrderResponse>>;
