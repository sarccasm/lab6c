using System.Collections.Generic;
using System.Threading.Tasks;
using ProductsValidation.Models;
namespace ProductsValidation.Services {
public interface ICustomerRepository {
 Task<List<Customer>> ListAsync(string search,string sort);
 Task<Customer> FindAsync(int id);
 Task<bool> HasOrdersAsync(int id);
 Task AddAsync(Customer customer);
 Task SaveAsync();
 Task DeleteAsync(Customer customer);
}
}