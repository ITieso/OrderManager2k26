namespace OrderManager.Application.DTOs;

/// <summary>
/// Represents an order item in the response.
/// </summary>
/// <param name="ProductName">The name of the product.</param>
/// <param name="Quantity">The quantity ordered.</param>
/// <param name="UnitPrice">The unit price of the product.</param>
/// <param name="TotalPrice">The calculated total price (Quantity * UnitPrice).</param>
public record OrderItemResponse(
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);
