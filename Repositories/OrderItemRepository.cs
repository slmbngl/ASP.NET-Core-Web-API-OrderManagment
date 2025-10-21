using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderManagementApi.Data;
using OrderManagementApi.Interfaces;
using OrderManagementApi.Models;

namespace OrderManagementApi.Repositories
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly AppDbContext _context;
        public OrderItemRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<OrderItem>> GetOrderIdAsync()
        {
            return await _context.OrderItems.ToListAsync();
        }
        public async Task<IEnumerable<OrderItem>> GetItemsByOrderIdAsync(int orderId)
        {
            // Belirli bir siparişe ait tüm kalemleri Product bilgisiyle birlikte çeker
            return await _context.OrderItems
                                 .Where(item => item.OrderId == orderId)
                                 .Include(item => item.Product)
                                 .ToListAsync();
        }
        public async Task<OrderItem?> GetItemByIdAsync(int itemId)
        {
            // Belirli bir kalemi Product bilgisiyle birlikte çeker
             return await _context.OrderItems
                                  .Include(item => item.Product)
                                  .FirstOrDefaultAsync(item => item.Id == itemId);
        }
    }
}