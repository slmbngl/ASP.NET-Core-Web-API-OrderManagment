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

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                                 .Include(o => o.Customer)
                                 .Include(o => o.OrderItems)
                                 .ThenInclude(oi => oi.Product)
                                 .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                                 .Include(o => o.Customer)
                                 .Include(o => o.OrderItems)
                                 .ThenInclude(oi => oi.Product)
                                 .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<bool> ExistsAsync(int id) => await _context.Orders.AnyAsync(o => o.Id == id);


        public async Task<Order> CreateOrderAsync(CreateOrderDto orderDto)
        {
            // 1. Müşteriyi çek (İlişkilendirme için)
            var customer = await _context.Customers.FindAsync(orderDto.CustomerId);
            if (customer == null)
            {
                throw new ArgumentException("Geçersiz Müşteri ID'si.");
            }

            // 2. Ürün ve Stok Kontrolleri
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
                    throw new InvalidOperationException($"Ürün '{product.Name}' için yetersiz stok. Mevcut: {product.StockQuantity}");

                // 3. OrderItem'ı oluştur
                var orderItem = new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price // DTO'daki atama düzeltildi
                };
                orderItems.Add(orderItem);
                totalAmount += orderItem.UnitPrice * orderItem.Quantity;

                // 4. Stokları Düşür 
                product.StockQuantity -= itemDto.Quantity;
            }

            // 5. Ana Sipariş Nesnesini Oluşturma
            var order = new Order
            {
                CustomerId = orderDto.CustomerId,
                Customer = customer, // KRİTİK EŞLEŞTİRME
                OrderItems = orderItems,
                TotalAmount = totalAmount,
                Status = "pending"
            };

            // 6. Kaydetme İşlemi (Transaction)
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // 7. İlişkili verilerle birlikte yeniden çekme (Controller dönüşümü için)
            var createdOrderWithRelations = await GetOrderByIdAsync(order.Id);

            return createdOrderWithRelations!;
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

        public async Task<bool> GetOrdersByApplicationUserIdAsync(string applicationUserId)
        {
            // 1. Customer ID'sini, ApplicationUserId'den bul
            var customer = await _context.Customers
                                         .FirstOrDefaultAsync(c => c.ApplicationUserId == applicationUserId);

            if (customer == null)
            {
                // Kullanıcının Customer kaydı yoksa boş liste döndür
                return false;
            }

            // 2. Customer ID'ye göre siparişleri filtrele ve ilişkili verileri yükle
            var hasOrders = await _context.Orders
                                  .AnyAsync(o => o.CustomerId == customer.Id);

            return hasOrders;
        }
    }
}