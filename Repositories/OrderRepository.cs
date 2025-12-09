using Microsoft.EntityFrameworkCore;
using OrderManagementApi.Data;
using OrderManagementApi.Interfaces;
using OrderManagementApi.Models;
using OrderManagementApi.DTOs;

namespace OrderManagementApi.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;
        private readonly IUserContextService _userContext;
        public OrderRepository(AppDbContext context, IUserContextService userContext)
        {

            _context = context;
            _userContext = userContext;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                                 .Include(o => o.Customer)
                                 .Include(o => o.OrderItems)
                                 .ThenInclude(oi => oi.Product)
                                 .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            var userId = _userContext.UserId;

            var customer = await _context.Customers
                                         .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (customer == null)
                return null;

            return await _context.Orders
                                 .Include(o => o.Customer)
                                 .Include(o => o.OrderItems)
                                 .ThenInclude(oi => oi.Product)
                                 .FirstOrDefaultAsync(o => o.Id == orderId && o.CustomerId == customer.Id);
        }

        public async Task<bool> ExistsAsync(int id) => await _context.Orders.AnyAsync(o => o.Id == id);


        public async Task<Order> CreateOrderAsync(CreateOrderDto orderDto)
        {
            var userId = _userContext.UserId;
            // Find the customer through ApplicationUserId
            var customer = await _context.Customers
                                         .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (customer == null)
                throw new InvalidOperationException("Bu kullanıcıya ait müşteri kaydı bulunamadı.");

            // Product and Stock control are the same way
            var productIds = orderDto.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var itemDto in orderDto.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == itemDto.ProductId);

                if (product == null)
                    throw new ArgumentException($"Ürün ID {itemDto.ProductId} bulunamadı.");

                if (product.StockQuantity < itemDto.Quantity)
                    throw new InvalidOperationException(
                        $"Ürün '{product.Name}' için yetersiz stok. Mevcut: {product.StockQuantity}");

                var orderItem = new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price
                };

                orderItems.Add(orderItem);
                totalAmount += orderItem.UnitPrice * orderItem.Quantity;

                product.StockQuantity -= itemDto.Quantity;
            }

            // Create Order
            var order = new Order
            {
                CustomerId = customer.Id,
                Customer = customer,
                OrderItems = orderItems,
                TotalAmount = totalAmount,
                Status = "pending"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();


            // Save the order and then create the OrderItems' TotalAmount   
            foreach (var item in order.OrderItems)
            {
                item.TotalAmount = order.TotalAmount;
            }

            await _context.SaveChangesAsync();

            return await GetOrderByIdAsync(order.Id);
        }

        public async Task UpdateOrderAsync(int id, string newStatus)
        {
            string normalizedNewStatus = newStatus.ToLower();

            var orderToUpdate = await _context.Orders
                                                .Include(o => o.OrderItems)
                                                .ThenInclude(oi => oi.Product)
                                                .FirstOrDefaultAsync(o => o.Id == id);

            if (orderToUpdate == null)
            {
                throw new KeyNotFoundException($"ID {id} ile sipariş bulunamadı.");
            }

            // Stok iadesi mantığı
            if (orderToUpdate.Status != "cancelled" && normalizedNewStatus == "cancelled")
            {
                foreach (var item in orderToUpdate.OrderItems)
                {
                    var product = item.Product;
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                    }
                }
            }

            orderToUpdate.Status = normalizedNewStatus;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteOrderWithStockReturnAsync(int id)
        {
            var order = await GetOrderByIdAsync(id);
            if (order == null)
            {
                throw new KeyNotFoundException($"ID {id} ile sipariş bulunamadı.");
            }

            // Stok İadesi İşlemi
            foreach (var item in order.OrderItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                }
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByApplicationUserIdAsync()
        {
            // Finf Customer ID and AplicationUserId
            var userId = _userContext.UserId;
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.ApplicationUserId == userId);
            if (customer == null) return new List<Order>();

            // Filter orders by customer ID and load data
            return await _context.Orders
                         .Where(o => o.CustomerId == customer.Id)
                         .Include(o => o.Customer)
                         .Include(o => o.OrderItems)
                         .ThenInclude(oi => oi.Product)
                         .ToListAsync();
        }
    }
}