namespace OrderManager.Application.DTOs;

public record OrderResponse(
    Guid Id,
    string PedidoId,
    List<OrderItemResponse> Items,
    decimal TotalAmount,
    decimal TaxAmount,
    string Status,
    DateTime CreatedAt,
    DateTime? ProcessedAt
);
