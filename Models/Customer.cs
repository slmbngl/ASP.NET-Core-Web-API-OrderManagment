using System.Text.Json.Serialization;
using OrderManagementApi.Models; // ApplicationUser için

public class Customer
{
    public int Id { get; set; }
    public string? FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;

    // YENİ ALAN 1: Identity User'a referans
    public string? ApplicationUserId { get; set; } = string.Empty; 

    // YENİ ALAN 2: Navigation property (Gerekli değil ama ilişkiyi kurar)
    public ApplicationUser ApplicationUser { get; set; } = default!; 

    // İlişki (Bir Müşteri birden fazla Sipariş verebilir)
    [JsonIgnore]
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}