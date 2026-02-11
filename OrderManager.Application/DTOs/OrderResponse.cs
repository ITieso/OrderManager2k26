namespace OrderManager.Application.DTOs;

/// <summary>
/// Response payload containing order details.
/// </summary>
/// <param name="Id">Internal order identifier (GUID).</param>
/// <param name="PedidoId">External order identifier from System A.</param>
/// <param name="Items">List of items in the order.</param>
/// <param name="TotalAmount">Total amount before tax.</param>
/// <param name="TaxAmount">Calculated tax amount.</param>
/// <param name="Status">Current order status.</param>
/// <param name="CreatedAt">Order creation timestamp.</param>
/// <param name="ProcessedAt">Order processing timestamp (null if not processed).</param>
public record OrderResponse(
    Guid Id,
    string PedidoId,
    IReadOnlyList<OrderItemResponse> Items,
    decimal TotalAmount,
    decimal TaxAmount,
    string Status,
    DateTime CreatedAt,
    DateTime? ProcessedAt
);
