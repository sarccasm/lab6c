using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductsValidation.Models;
using ProductsValidation.Services;
namespace ProductsValidation.Controllers {
public class SupermarketsController : Controller {
 readonly ShoppingContext db;
 public SupermarketsController(ShoppingContext context) {db=context;}
 public async Task<IActionResult> Index(string search,string sort,int page=1) { const int size=5; var count=await db.Supermarkets.CountAsync();
 var pages=System.Math.Max(1,(count+size-1)/size); page=System.Math.Clamp(page,1,pages);
 ViewBag.Page=page; ViewBag.Pages=pages;
 return View(await db.Supermarkets.OrderBy(s=>s.Name).ThenBy(s=>s.Id).Skip((page-1)*size).Take(size).ToListAsync()); }
 public async Task<IActionResult> Details(int id) {var item=await db.Supermarkets.FindAsync(id);return item==null?NotFound():View(item);}
 public IActionResult Create() => View("Edit",new Supermarket());
 public async Task<IActionResult> Edit(int id) {var item=await db.Supermarkets.FindAsync(id);return item==null?NotFound():View(item);}
 [HttpPost,ValidateAntiForgeryToken]
 public async Task<IActionResult> Create([Bind("Name,Address")] Supermarket item) {
 if(!ModelState.IsValid) return View("Edit",item);
 db.Supermarkets.Add(item);await db.SaveChangesAsync();return RedirectToAction(nameof(Index));}
 [HttpPost,ValidateAntiForgeryToken]
 public async Task<IActionResult> Edit(int id,[Bind("Name,Address")] Supermarket item) {
 var current=await db.Supermarkets.FindAsync(id);if(current==null)return NotFound();
 item.Id=id;if(!ModelState.IsValid)return View(item);
 current.Name=item.Name;current.Address=item.Address;
 await db.SaveChangesAsync();return RedirectToAction(nameof(Index));}
 public async Task<IActionResult> Delete(int id) {var item=await db.Supermarkets.FindAsync(id);return item==null?NotFound():View(item);}
 [HttpPost,ActionName("Delete"),ValidateAntiForgeryToken]
 public async Task<IActionResult> DeleteConfirmed(int id) {
 var item=await db.Supermarkets.FindAsync(id);if(item==null)return NotFound();
 if(await db.Orders.AnyAsync(o=>o.SupermarketId==id)) {ModelState.AddModelError("","Запис використовується в замовленні.");return View("Delete",item);}
 db.Supermarkets.Remove(item);await db.SaveChangesAsync();return RedirectToAction(nameof(Index));}
}
}