using MediatR;
using OrderManager.Application.DTOs;
using OrderManager.Domain.Common;

namespace OrderManager.Application.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid Id) : IRequest<Result<OrderResponse>>;
