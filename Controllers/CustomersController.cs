using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProductsValidation.Models;
using ProductsValidation.Services;
namespace ProductsValidation.Controllers {
public class CustomersController : Controller {
 readonly ICustomerRepository repository;
 public CustomersController(ICustomerRepository repository) {this.repository=repository;}
 public async Task<IActionResult> Index(string search,string sort,int page=1) {
 ViewBag.Search=search;ViewBag.Sort=sort;
 return View(await repository.ListAsync(search,sort));
 }
 public async Task<IActionResult> Details(int id) {var item=await repository.FindAsync(id);return item==null?NotFound():View(item);}
 public IActionResult Create() => View("Edit",new Customer());
 public async Task<IActionResult> Edit(int id) {var item=await repository.FindAsync(id);return item==null?NotFound():View(item);}
 [HttpPost,ValidateAntiForgeryToken]
 public async Task<IActionResult> Create([Bind("FirstName,LastName,Address,Discount")] Customer item) {
 if(!ModelState.IsValid) return View("Edit",item);
 await repository.AddAsync(item);return RedirectToAction(nameof(Index));}
 [HttpPost,ValidateAntiForgeryToken]
 public async Task<IActionResult> Edit(int id,[Bind("FirstName,LastName,Address,Discount")] Customer item) {
 var current=await repository.FindAsync(id);if(current==null)return NotFound();
 item.Id=id;if(!ModelState.IsValid)return View(item);
 current.FirstName=item.FirstName;current.LastName=item.LastName;current.Address=item.Address;current.Discount=item.Discount;
 await repository.SaveAsync();return RedirectToAction(nameof(Index));}
 public async Task<IActionResult> Delete(int id) {var item=await repository.FindAsync(id);return item==null?NotFound():View(item);}
 [HttpPost,ActionName("Delete"),ValidateAntiForgeryToken]
 public async Task<IActionResult> DeleteConfirmed(int id) {
 var item=await repository.FindAsync(id);if(item==null)return NotFound();
 if(await repository.HasOrdersAsync(id)) {ModelState.AddModelError("","Запис використовується в замовленні.");return View("Delete",item);}
 await repository.DeleteAsync(item);return RedirectToAction(nameof(Index));}
}
}