using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ProductsValidation.Models;
namespace ProductsValidation.Services {
public static class SampleData {
 public static void Initialize(System.IServiceProvider services) {
 using var scope=services.CreateScope();
 var db=scope.ServiceProvider.GetRequiredService<ShoppingContext>();
 db.Database.EnsureCreated();
 if(db.Customers.Any()) return;
 var customer=new Customer {FirstName="Іван",LastName="Петренко",Address="Чернівці",Discount=0};
 db.Customers.AddRange(customer,new Customer {FirstName="Олена",LastName="Коваль",Address="Київ",Discount=5});
 for(int i=1;i<=12;i++) db.Supermarkets.Add(new Supermarket {Name="Супермаркет "+i,Address="вул. Головна, "+i});
 db.Products.AddRange(new Data().Products);
 db.SaveChanges();
 var product=db.Products.OrderBy(p=>p.Id).First();
 db.Orders.Add(new Order {CustomerId=customer.Id,SupermarketId=db.Supermarkets.OrderBy(s=>s.Id).First().Id,OrderDetails={new OrderDetails {ProductId=product.Id,Quantity=2,UnitPrice=product.Price}}});
 db.SaveChanges();
 }
}
}