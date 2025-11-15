using OrderManagementApi.DTOs;
using OrderManagementApi.Models;
namespace OrderManagementApi.Interfaces
{
    public interface IOrderRepository
    {
        
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order?> GetOrderByIdAsync(int id);
        Task<Order> CreateOrderAsync(CreateOrderDto orderDto);
        Task UpdateOrderAsync(int id, string newStatus);
        Task DeleteOrderWithStockReturnAsync(int id);
         Task<bool> ExistsAsync(int id);
        Task<bool> GetOrdersByApplicationUserIdAsync(string userId);
    }
}