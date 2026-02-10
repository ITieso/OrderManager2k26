namespace OrderManager.Application.DTOs;

public record OrderItemRequest(
    string ProductName,
    int Quantity,
    decimal UnitPrice
);
