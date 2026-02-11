using System.ComponentModel.DataAnnotations;

namespace OrderManager.Application.DTOs;

/// <summary>
/// Request payload for an order item.
/// </summary>
/// <param name="ProductName">The name of the product.</param>
/// <param name="Quantity">The quantity to order (must be greater than 0).</param>
/// <param name="UnitPrice">The unit price (must be greater than 0).</param>
public record OrderItemRequest(
    [Required] string ProductName,
    [Range(1, int.MaxValue)] int Quantity,
    [Range(0.01, double.MaxValue)] decimal UnitPrice
);
