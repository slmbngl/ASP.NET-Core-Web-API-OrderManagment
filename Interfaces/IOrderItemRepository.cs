using OrderManagementApi.Models;

namespace OrderManagementApi.Interfaces
{
    public interface IOrderItemRepository
    {
        Task<IEnumerable<OrderItem>> GetItemsByOrderIdAsync(int orderId);
        Task<IEnumerable<OrderItem>> GetOrderIdAsync();
        Task<OrderItem?> GetItemByIdAsync(int itemId);
    }
}