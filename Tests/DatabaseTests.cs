using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProductsValidation.Models;
using ProductsValidation.Services;
using Xunit;
namespace Shopping.Tests {
public class DatabaseTests : IDisposable {
 readonly SqliteConnection connection=new("Data Source=:memory:");
 readonly DbContextOptions<ShoppingContext> options;
 public DatabaseTests(){connection.Open();options=new DbContextOptionsBuilder<ShoppingContext>().UseSqlite(connection).Options;using var db=Context();db.Database.EnsureCreated();}
 ShoppingContext Context()=>new(options);
 public void Dispose()=>connection.Dispose();
 [Fact] public async Task CustomerCrudPersistsAcrossContexts(){
 int id;using(var db=Context()){var repo=new CustomerRepository(db);var c=new Customer{FirstName="Ivan",LastName="Petrenko",Address="Chernivtsi"};await repo.AddAsync(c);id=c.Id;Assert.True(id>0);}
 using(var db=Context()){var repo=new CustomerRepository(db);var c=await repo.FindAsync(id);Assert.Equal("Ivan",c.FirstName);c.Address="Kyiv";await repo.SaveAsync();}
 using(var db=Context()){var repo=new CustomerRepository(db);var c=await repo.FindAsync(id);Assert.Equal("Kyiv",c.Address);await repo.DeleteAsync(c);}
 using(var db=Context()){Assert.Null(await new CustomerRepository(db).FindAsync(id));}
 }
 [Theory][InlineData("", "", "Adams")][InlineData("", "name_desc", "Brown")][InlineData("", "address", "Brown")][InlineData("", "address_desc", "Adams")][InlineData("Ann", "", "Adams")][InlineData("row", "", "Brown")]
 public async Task SearchAndSortUseDatabase(string search,string sort,string first){
 using var db=Context();db.Customers.AddRange(new Customer{FirstName="Ann",LastName="Adams",Address="Zurich"},new Customer{FirstName="Bob",LastName="Brown",Address="Amsterdam"});await db.SaveChangesAsync();
 var result=await new CustomerRepository(db).ListAsync(search,sort);Assert.Equal(first,result[0].LastName);Assert.Equal(search==""?2:1,result.Count);
 }
 [Fact] public async Task ProductAndSupermarketCrudPersists(){
 int productId,marketId;using(var db=Context()){var p=new Product{Name="Milk",Price=25.50m};var s=new Supermarket{Name="Market",Address="Main street"};db.AddRange(p,s);await db.SaveChangesAsync();productId=p.Id;marketId=s.Id;}
 using(var db=Context()){var p=await db.Products.FindAsync(productId);var s=await db.Supermarkets.FindAsync(marketId);Assert.Equal(25.50m,p.Price);Assert.Equal("Market",s.Name);p.Price=30;s.Address="New street";await db.SaveChangesAsync();}
 using(var db=Context()){var p=await db.Products.FindAsync(productId);var s=await db.Supermarkets.FindAsync(marketId);Assert.Equal(30m,p.Price);Assert.Equal("New street",s.Address);db.RemoveRange(p,s);await db.SaveChangesAsync();}
 using(var db=Context()){Assert.Empty(await db.Products.ToListAsync());Assert.Empty(await db.Supermarkets.ToListAsync());}
 }
 async Task<int> SeedOrder(){using var db=Context();var c=new Customer{FirstName="Ann",LastName="Adams",Address="Kyiv"};var s=new Supermarket{Name="Market",Address="Main"};var p=new Product{Name="Milk",Price=25.50m};var o=new Order{Customer=c,Supermarket=s,OrderDetails={new OrderDetails{Product=p,Quantity=2.5m,UnitPrice=p.Price}}};db.Orders.Add(o);await db.SaveChangesAsync();return o.Id;}
 [Fact] public async Task OrderCrudLoadsRelationsAndCascadesDetails(){
 int id=await SeedOrder();
 using(var db=Context()){var o=await db.Orders.Include(o=>o.Customer).Include(o=>o.Supermarket).Include(o=>o.OrderDetails).ThenInclude(d=>d.Product).SingleAsync();Assert.Equal("Ann",o.Customer.FirstName);Assert.Equal("Market",o.Supermarket.Name);Assert.Equal("Milk",o.OrderDetails.Single().Product.Name);Assert.Equal(63.75m,o.OrderDetails.Sum(d=>d.Quantity*d.UnitPrice));o.OrderDetails.Single().Quantity=4;await db.SaveChangesAsync();}
 using(var db=Context()){var o=await db.Orders.Include(o=>o.OrderDetails).SingleAsync();Assert.Equal(4m,o.OrderDetails.Single().Quantity);db.Orders.Remove(o);await db.SaveChangesAsync();}
 using(var db=Context()){Assert.Empty(await db.Orders.ToListAsync());Assert.Empty(await db.OrderDetails.ToListAsync());Assert.Single(await db.Products.ToListAsync());}
 }
 [Theory][InlineData("customer")][InlineData("supermarket")][InlineData("product")]
 public async Task ForeignKeysPreventDeletingReferencedRecords(string entity){await SeedOrder();using var db=Context();
 if(entity=="customer")db.Customers.Remove(await db.Customers.SingleAsync());else if(entity=="supermarket")db.Supermarkets.Remove(await db.Supermarkets.SingleAsync());else db.Products.Remove(await db.Products.SingleAsync());
 await Assert.ThrowsAsync<DbUpdateException>(()=>db.SaveChangesAsync());}
 [Fact] public async Task HasOrdersReflectsActualRelations(){await SeedOrder();using var db=Context();var repo=new CustomerRepository(db);Assert.True(await repo.HasOrdersAsync((await db.Customers.SingleAsync()).Id));Assert.False(await repo.HasOrdersAsync(999));}
 [Fact] public async Task ForeignKeysRejectNonexistentOrderReferences(){using var db=Context();db.Orders.Add(new Order{CustomerId=999,SupermarketId=999});await Assert.ThrowsAsync<DbUpdateException>(()=>db.SaveChangesAsync());}
}
}
