using System.Text.Json.Serialization;

namespace OrderManagementApi.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }

        // Relationships
        [JsonIgnore]
        public Order Order { get; set; }
        [JsonIgnore]
        public Product Product { get; set; }
    }
}