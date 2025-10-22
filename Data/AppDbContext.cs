using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrderManagementApi.Models; // ApplicationUser, Customer, Order, vb.

namespace OrderManagementApi.Data
{
    // IdentityDbContext kullanıyoruz ve ApplicationUser modelimizi belirtiyoruz.
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Uygulama Modelleri için DbSet'ler
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Identity'nin kendi tablolarının (AspNetUsers, AspNetRoles vb.) yapılandırılması için bu satır KRİTİKTİR.
            base.OnModelCreating(builder);

            // *******************************************************************
            // UYGULAMA İLİŞKİLERİ VE KISITLAMALAR
            // *******************************************************************

            builder.Entity<Customer>()
        .HasOne(c => c.ApplicationUser)          // Customer'ın bir ApplicationUser'ı var
        .WithOne(au => au.Customer)              // ApplicationUser'ın bir Customer'ı var
        .HasForeignKey<Customer>(c => c.ApplicationUserId) // Customer'daki ApplicationUserId foreign key'dir
        .IsRequired()
        .OnDelete(DeleteBehavior.Cascade);
            // 1. Order <-> OrderItem (Cascade Delete)
            // Bir Order silinirse, ona ait tüm OrderItem'lar silinir.
            builder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. Customer <-> Order (Cascade Delete)
            // Bir Customer silinirse, ona ait tüm Order'lar silinir.
            builder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. OrderItem <-> Product (Kısıtla, ürünü silmek sipariş kalemini silmemeli, 
            // ama sipariş kalemi olan ürün silinememeli)
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany() // Ürün tarafında OrderItem koleksiyonu tutmuyorsanız (tercih edilen)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // Ürünün yanlışlıkla silinmesini engeller.

            // Eğer OrderItem'da UnitPriceSnapshot yerine UnitPrice kullanıyorsanız, 
            // modeldeki doğru alan adını kullanın.
            builder.Entity<OrderItem>()
                .Property(oi => oi.UnitPrice)
                .HasColumnType("decimal(18, 2)");

            // Product'taki Price ve Order'daki TotalAmount için de tip tanımı yapılabilir (isteğe bağlı ama önerilir)
            builder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18, 2)");

            builder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18, 2)");
        }
    }
}