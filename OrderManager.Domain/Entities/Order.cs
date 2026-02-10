using OrderManager.Domain.Enums;

namespace OrderManager.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public string PedidoId { get; private set; } = string.Empty;
    public List<OrderItem> Items { get; private set; } = new();
    public decimal TotalAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    private Order() { }

    public static Order Create(string pedidoId, List<OrderItem> items)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            PedidoId = pedidoId,
            Items = items,
            TotalAmount = items.Sum(i => i.TotalPrice),
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        return order;
    }

    public void ApplyTax(decimal taxAmount)
    {
        TaxAmount = taxAmount;
    }

    public void MarkAsProcessing()
    {
        Status = OrderStatus.Processing;
    }

    public void MarkAsProcessed()
    {
        Status = OrderStatus.Processed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed()
    {
        Status = OrderStatus.Failed;
    }
}
