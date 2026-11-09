using Microsoft.EntityFrameworkCore;
using ProductsValidation.Models;
namespace ProductsValidation.Services {
public class ShoppingContext : DbContext {
 public ShoppingContext(DbContextOptions<ShoppingContext> options):base(options) {}
 public DbSet<Product> Products {get;set;}
 public DbSet<Customer> Customers {get;set;}
 public DbSet<Supermarket> Supermarkets {get;set;}
 public DbSet<Order> Orders {get;set;}
 public DbSet<OrderDetails> OrderDetails {get;set;}
 protected override void OnModelCreating(ModelBuilder b) {
 b.Entity<Product>().Property(p=>p.Name).HasMaxLength(50).IsRequired();
 b.Entity<Product>().Property(p=>p.Price).HasPrecision(18,2);
 b.Entity<OrderDetails>().Property(p=>p.UnitPrice).HasPrecision(18,2);
 b.Entity<OrderDetails>().Property(p=>p.Quantity).HasPrecision(18,3);
 b.Entity<Order>().HasOne(o=>o.Customer).WithMany(c=>c.Orders).HasForeignKey(o=>o.CustomerId).OnDelete(DeleteBehavior.Restrict);
 b.Entity<Order>().HasOne(o=>o.Supermarket).WithMany(s=>s.Orders).HasForeignKey(o=>o.SupermarketId).OnDelete(DeleteBehavior.Restrict);
 b.Entity<OrderDetails>().HasOne(d=>d.Product).WithMany().HasForeignKey(d=>d.ProductId).OnDelete(DeleteBehavior.Restrict);
 b.Entity<OrderDetails>().HasOne(d=>d.Order).WithMany(o=>o.OrderDetails).HasForeignKey(d=>d.OrderId).OnDelete(DeleteBehavior.Cascade);
 }
}
}