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
            return await _context.OrderItems
                                 .Where(item => item.OrderId == orderId)
                                 .Include(item => item.Product)
                                 .ToListAsync();
        }
        public async Task<OrderItem?> GetItemByIdAsync(int itemId)
        {
            return await _context.OrderItems
                                 .Include(item => item.Product)
                                 .FirstOrDefaultAsync(item => item.Id == itemId);
        }
        public async Task<IEnumerable<OrderItem>> GetItemsByApplicationUserIdAsync(string applicationUserId)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == applicationUserId);
            if (customer == null) return new List<OrderItem>();

            var orderIds = await _context.Orders
                                            .Where(o => o.CustomerId == customer.Id)
                                            .Select(o => o.Id)
                                            .ToListAsync();

            if (!orderIds.Any())
                return new List<OrderItem>();
            return await _context.OrderItems
                                     .Where(oi => orderIds.Contains(oi.OrderId))
                                     .ToListAsync();

        }
    }
}