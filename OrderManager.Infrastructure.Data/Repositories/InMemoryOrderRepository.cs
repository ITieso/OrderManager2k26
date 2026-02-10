using System.Collections.Concurrent;
using OrderManager.Domain.Entities;
using OrderManager.Domain.Enums;
using OrderManager.Domain.Interfaces;

namespace OrderManager.Infrastructure.Data.Repositories;

public class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();

    public Task<Order?> GetByIdAsync(Guid id)
    {
        _orders.TryGetValue(id, out var order);
        return Task.FromResult(order);
    }

    public Task<Order?> GetByPedidoIdAsync(string pedidoId)
    {
        var order = _orders.Values.FirstOrDefault(o => o.PedidoId == pedidoId);
        return Task.FromResult(order);
    }

    public Task<IEnumerable<Order>> GetAllAsync()
    {
        return Task.FromResult(_orders.Values.AsEnumerable());
    }

    public Task<IEnumerable<Order>> GetProcessedOrdersAsync()
    {
        var processedOrders = _orders.Values
            .Where(o => o.Status == OrderStatus.Processed);
        return Task.FromResult(processedOrders);
    }

    public Task<bool> ExistsAsync(string pedidoId)
    {
        var exists = _orders.Values.Any(o => o.PedidoId == pedidoId);
        return Task.FromResult(exists);
    }

    public Task AddAsync(Order order)
    {
        _orders.TryAdd(order.Id, order);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Order order)
    {
        _orders[order.Id] = order;
        return Task.CompletedTask;
    }
}
