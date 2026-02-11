using System.ComponentModel.DataAnnotations;

namespace OrderManager.Application.DTOs;

/// <summary>
/// Request payload for creating a new order.
/// </summary>
/// <param name="PedidoId">External order identifier from System A.</param>
/// <param name="Items">List of items in the order.</param>
public record CreateOrderRequest(
    [Required] string PedidoId,
    [Required, MinLength(1)] List<OrderItemRequest> Items
);
