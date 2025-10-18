using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace OrderManagementApi.DTOs
{
    public class CreateOrderDto
    {
        [Required]
        public int CustomerId { get; set; }

        // Bire-Çok İlişki: Sipariş, bir OrderItem listesi içerir.
        [Required]
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }

    public class OrderResponseDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }

        // İlişkileri basitleştirin. Customer'ın sadece ID'sini veya adını döndürün.
        public int CustomerId { get; set; }
        public string CustomerFullName { get; set; }

        public ICollection<OrderItemResponseDto> Items { get; set; }
    }
    public class OrderItemResponseDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } // Ek bilgi
        public int Quantity { get; set; }
        public decimal UnitPriceSnapshot { get; set; }
    }

    public class UpdateOrderStatusDto
    {
        [Required]
        public string Status { get; set; } // Örn: Shipped, Delivered, Cancelled
    }
}