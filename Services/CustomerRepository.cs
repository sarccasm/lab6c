using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProductsValidation.Models;
namespace ProductsValidation.Services {
public class CustomerRepository : ICustomerRepository {
 readonly ShoppingContext db;
 public CustomerRepository(ShoppingContext context) {db=context;}
 public async Task<List<Customer>> ListAsync(string search,string sort) {
 var query=db.Customers.AsQueryable();
 if(!string.IsNullOrWhiteSpace(search))query=query.Where(c=>c.FirstName.Contains(search)||c.LastName.Contains(search));
 query=sort switch {"name_desc"=>query.OrderByDescending(c=>c.LastName),"address"=>query.OrderBy(c=>c.Address),"address_desc"=>query.OrderByDescending(c=>c.Address),_=>query.OrderBy(c=>c.LastName)};
 return await query.ToListAsync();
 }
 public Task<Customer> FindAsync(int id)=>db.Customers.FindAsync(id).AsTask();
 public Task<bool> HasOrdersAsync(int id)=>db.Orders.AnyAsync(o=>o.CustomerId==id);
 public async Task AddAsync(Customer customer){db.Customers.Add(customer);await db.SaveChangesAsync();}
 public async Task SaveAsync(){await db.SaveChangesAsync();}
 public async Task DeleteAsync(Customer customer){db.Customers.Remove(customer);await db.SaveChangesAsync();}
}
}