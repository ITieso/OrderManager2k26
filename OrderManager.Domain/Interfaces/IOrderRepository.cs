using OrderManager.Domain.Entities;

namespace OrderManager.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<Order?> GetByPedidoIdAsync(string pedidoId);
    Task<IEnumerable<Order>> GetAllAsync();
    Task<IEnumerable<Order>> GetProcessedOrdersAsync();
    Task<bool> ExistsAsync(string pedidoId);
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
}
