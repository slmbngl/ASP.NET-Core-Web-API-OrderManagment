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

        // --- OKUMA METOTLARI (Aynı Kalır) ---
        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders.ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _context.Orders

                                 .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Orders.AnyAsync(o => o.Id == id);
        }

        // --- KARMAŞIK YAZMA METOTLARI ---

        public async Task<Order> CreateOrderAsync(CreateOrderDto orderDto)
        {
            // 1. Müşteri Kontrolü (Doğrudan DbContext üzerinden)
            var customerExists = await _context.Customers.AnyAsync(c => c.Id == orderDto.CustomerId);
            if (!customerExists)
            {
                // İş mantığı hatalarını fırlatıyoruz, Controller bunu yakalayıp BadRequest döndürecek.
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
                    UnitPrice = product.Price
                };
                orderItems.Add(orderItem);
                totalAmount += orderItem.UnitPrice * orderItem.Quantity;

                // 4. Stokları Düşür (Change Tracker'a kaydolur)
                product.StockQuantity -= itemDto.Quantity;
            }

            // 5. Ana Sipariş Nesnesini Oluşturma
            var order = new Order
            {
                CustomerId = orderDto.CustomerId,
                OrderItems = orderItems,
                TotalAmount = totalAmount,
                Status = "Pending"
            };

            // 6. Kaydetme İşlemi (Tüm değişiklikler tek bir Transaction'da)
            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // Stoklar ve Sipariş burada aynı anda kaydedilir.

            return order;
        }

        // Basit Update Metodu (Güncelleme için DTO'lar kullanılmadığı varsayımıyla)
        public async Task UpdateOrderAsync(int id, string newStatus)
        {
            // YENİ DURUMU KÜÇÜK HARFE ÇEVİRİYORUZ (Tutarlılık için)
            string normalizedNewStatus = newStatus.ToLower();

            // 1. Siparişi OrderItems ve Product bilgisiyle birlikte çek (Include kullanarak!)
            var orderToUpdate = await _context.Orders
                                                .Include(o => o.OrderItems) // Kalemleri yükle
                                                .ThenInclude(oi => oi.Product) // Kalemlerin ürünlerini yükle
                                                .FirstOrDefaultAsync(o => o.Id == id);

            if (orderToUpdate == null)
            {
                throw new KeyNotFoundException($"ID {id} ile sipariş bulunamadı.");
            }

            // 2. İptal Kontrolü ve Stok İadesi İş Mantığı
            // Sadece mevcut durum "cancelled" değilse VE yeni durum "cancelled" ise stok iadesi yap.
            if (orderToUpdate.Status != "cancelled" && normalizedNewStatus == "cancelled")
            {
                // Stok iadesi yap: Her bir sipariş kalemi için döngüye gir
                foreach (var item in orderToUpdate.OrderItems)
                {
                    // Product nesnesi zaten Include ile yüklendiği için direkt kullanabiliriz.
                    var product = item.Product;

                    // Eğer ürün bilgisi yüklendiyse ve stok miktarını güncelleyebiliriz
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                    }
                    // NOT: EF Core, product nesnesindeki bu değişikliği otomatik takip eder.
                }
            }

            // 3. Sipariş Durumunu Güncelleme
            orderToUpdate.Status = normalizedNewStatus;

            // 4. Değişiklikleri Kaydetme
            // Bu tek SaveChanges, hem sipariş durumunu hem de stok güncellemelerini DB'ye yansıtır.
            await _context.SaveChangesAsync();
        }

        // DELETE metodu: Stok iadesi iş mantığını içerir
        public async Task DeleteOrderWithStockReturnAsync(int id)
        {
            // Önce Order'ı ve kalemlerini çek (Include ile çekilmiş olmalı)
            var order = await GetOrderByIdAsync(id);
            if (order == null)
            {
            }

            // Stok İadesi İşlemi
            foreach (var item in order.OrderItems)
            {
                // Product'ı çekip, EF'in takibine al
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity; // Stok iade edildi
                                                            // Kaydetmeye gerek yok, alttaki SaveChanges hepsini halledecek
                }
            }

            // Siparişi sil
            _context.Orders.Remove(order);

            // Tüm değişiklikleri tek seferde kaydet (Sipariş silme + Stok artırma)
            await _context.SaveChangesAsync();
        }

    }
}