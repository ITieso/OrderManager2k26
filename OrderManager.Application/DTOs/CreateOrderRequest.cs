namespace OrderManager.Application.DTOs;

public record CreateOrderRequest(
    string PedidoId,
    List<OrderItemRequest> Items
);
