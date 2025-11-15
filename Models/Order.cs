using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OrderManagementApi.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending"; // Örn: Pending, Shipped, Delivered, Cancelled

        // İlişkiler
        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = default!;

        // Bire-çok ilişki: Bir siparişin birden çok kalemi vardır.
        [JsonIgnore]
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}