namespace OrderManagementApi.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        
        // Bir ürün birden fazla OrderItem'da yer alabilir (isteğe bağlı, şimdilik zorunlu değil)
        // public ICollection<OrderItem> OrderItems { get; set; }
    }
}