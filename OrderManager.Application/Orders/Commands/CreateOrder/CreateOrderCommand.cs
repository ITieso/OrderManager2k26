using MediatR;
using OrderManager.Application.DTOs;
using OrderManager.Domain.Common;

namespace OrderManager.Application.Orders.Commands.CreateOrder;

public record CreateOrderCommand(
    string PedidoId,
    List<OrderItemRequest> Items
) : IRequest<Result<OrderResponse>>;
