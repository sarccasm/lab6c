using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProductsValidation.Models;
using ProductsValidation.Services;
namespace ProductsValidation.Controllers {
public class OrdersController : Controller {
 readonly ShoppingContext db;
 public OrdersController(ShoppingContext context) {db=context;}
 public async Task<IActionResult> Index() => View(await db.Orders.Include(o=>o.Customer).Include(o=>o.Supermarket).OrderByDescending(o=>o.OrderDate).ThenByDescending(o=>o.Id).ToListAsync());
 public async Task<IActionResult> Details(int id) {
 var order=await db.Orders.Include(o=>o.Customer).Include(o=>o.Supermarket).Include(o=>o.OrderDetails).ThenInclude(d=>d.Product).FirstOrDefaultAsync(o=>o.Id==id);
 return order==null?NotFound():View(order);
 }
 public async Task<IActionResult> Create() {var model=new OrderForm();await Populate(model);return View("Edit",model);}
 public async Task<IActionResult> Edit(int id) {
 var order=await db.Orders.Include(o=>o.OrderDetails).FirstOrDefaultAsync(o=>o.Id==id);
 if(order==null)return NotFound();
 var model=new OrderForm {Id=id,CustomerId=order.CustomerId,SupermarketId=order.SupermarketId,OrderDate=order.OrderDate,
 Lines=order.OrderDetails.Select(d=>new OrderLineForm {ProductId=d.ProductId,Quantity=d.Quantity}).ToList()};
 await Populate(model);return View(model);
 }
 [HttpPost,ValidateAntiForgeryToken]
 public Task<IActionResult> Create(OrderForm model) {model.Id=0;return Save(model,0);}
 [HttpPost,ValidateAntiForgeryToken]
 public Task<IActionResult> Edit(int id,OrderForm model) => Save(model,id);
 private async Task<IActionResult> Save(OrderForm model,int id) {
 var order=id==0?new Order():await db.Orders.Include(o=>o.OrderDetails).FirstOrDefaultAsync(o=>o.Id==id);
 if(order==null)return NotFound();model.Id=id;
 if(!await db.Customers.AnyAsync(c=>c.Id==model.CustomerId))ModelState.AddModelError("CustomerId","Оберіть покупця.");
 if(!await db.Supermarkets.AnyAsync(s=>s.Id==model.SupermarketId))ModelState.AddModelError("SupermarketId","Оберіть супермаркет.");
 model.Lines ??= new System.Collections.Generic.List<OrderLineForm>();
 var selected=model.Lines.Where(l=>l.Quantity>0).ToList();
 var products=await db.Products.ToDictionaryAsync(p=>p.Id);
 if(selected.Count==0)ModelState.AddModelError("","Додайте хоча б один товар із кількістю більшою за нуль.");
 if(model.Lines.Any(l=>!products.ContainsKey(l.ProductId))||model.Lines.Select(l=>l.ProductId).Distinct().Count()!=model.Lines.Count)ModelState.AddModelError("","Некоректний список товарів.");
 if(!ModelState.IsValid){
 foreach(var key in ModelState.Keys.Where(k=>k.StartsWith("Lines[")).ToList()) {
 var messages=ModelState[key].Errors.Select(e=>string.IsNullOrEmpty(e.ErrorMessage)?"Некоректна кількість товару.":e.ErrorMessage).ToList();
 ModelState.Remove(key); foreach(var message in messages)ModelState.AddModelError("",message);
 }
 await Populate(model);return View("Edit",model);
 }
 order.CustomerId=model.CustomerId;order.SupermarketId=model.SupermarketId;order.OrderDate=model.OrderDate;
 var prices=order.OrderDetails.ToDictionary(d=>d.ProductId,d=>d.UnitPrice);
 db.OrderDetails.RemoveRange(order.OrderDetails);order.OrderDetails.Clear();
 foreach(var line in selected)order.OrderDetails.Add(new OrderDetails {ProductId=line.ProductId,Quantity=line.Quantity,UnitPrice=prices.TryGetValue(line.ProductId,out var price)?price:products[line.ProductId].Price});
 if(id==0)db.Orders.Add(order);
 await db.SaveChangesAsync();return RedirectToAction(nameof(Details),new {id=order.Id});
 }
 private async Task Populate(OrderForm model) {
 ViewBag.Customers=new SelectList(await db.Customers.OrderBy(c=>c.LastName).ToListAsync(),"Id","FullName",model.CustomerId);
 ViewBag.Supermarkets=new SelectList(await db.Supermarkets.OrderBy(s=>s.Name).ToListAsync(),"Id","Name",model.SupermarketId);
 var quantities=model.Lines.GroupBy(l=>l.ProductId).ToDictionary(g=>g.Key,g=>g.First().Quantity);
 model.Lines=(await db.Products.OrderBy(p=>p.Name).ToListAsync()).Select(p=>new OrderLineForm {ProductId=p.Id,Name=p.Name,Price=p.Price,Quantity=quantities.GetValueOrDefault(p.Id)}).ToList();
 }
 public async Task<IActionResult> Delete(int id) {var order=await db.Orders.FindAsync(id);return order==null?NotFound():View(order);}
 [HttpPost,ActionName("Delete"),ValidateAntiForgeryToken]
 public async Task<IActionResult> DeleteConfirmed(int id) {
 var order=await db.Orders.FindAsync(id);if(order==null)return NotFound();db.Orders.Remove(order);await db.SaveChangesAsync();return RedirectToAction(nameof(Index));
 }
}
}
