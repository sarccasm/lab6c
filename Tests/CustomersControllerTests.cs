using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProductsValidation.Controllers;
using ProductsValidation.Models;
using ProductsValidation.Services;
using Xunit;
namespace Shopping.Tests {
public class CustomersControllerTests {
 readonly Mock<ICustomerRepository> repo=new(MockBehavior.Strict);
 CustomersController Controller()=>new(repo.Object);
 static Customer Customer()=>new() {Id=7,FirstName="Ivan",LastName="Petrenko",Address="Chernivtsi",Discount=5};
 [Fact] public async Task IndexPassesSearchAndSortAndReturnsCustomers(){
 var customers=new List<Customer>{Customer()};repo.Setup(r=>r.ListAsync("Ivan","address_desc")).ReturnsAsync(customers);
 var controller=Controller();var result=Assert.IsType<ViewResult>(await controller.Index("Ivan","address_desc"));
 Assert.Same(customers,result.Model);Assert.Equal("Ivan",(string)controller.ViewBag.Search);repo.VerifyAll();}
 [Theory][InlineData("details")][InlineData("edit")][InlineData("delete")]
 public async Task ReadActionsReturnNotFoundWhenMissing(string action){repo.Setup(r=>r.FindAsync(7)).ReturnsAsync((Customer)null);
 var c=Controller();var result=action=="details"?await c.Details(7):action=="edit"?await c.Edit(7):await c.Delete(7);
 Assert.IsType<NotFoundResult>(result);repo.VerifyAll();}
 [Theory][InlineData("details")][InlineData("edit")][InlineData("delete")]
 public async Task ReadActionsReturnCustomer(string action){var customer=Customer();repo.Setup(r=>r.FindAsync(7)).ReturnsAsync(customer);
 var c=Controller();var result=action=="details"?await c.Details(7):action=="edit"?await c.Edit(7):await c.Delete(7);
 Assert.Same(customer,Assert.IsType<ViewResult>(result).Model);repo.VerifyAll();}
 [Fact] public void CreateGetReturnsEmptyForm(){var result=Assert.IsType<ViewResult>(Controller().Create());Assert.Equal("Edit",result.ViewName);Assert.Equal(0,Assert.IsType<Customer>(result.Model).Id);}
 [Fact] public async Task CreateValidSavesAndRedirects(){var customer=Customer();repo.Setup(r=>r.AddAsync(customer)).Returns(Task.CompletedTask);
 Assert.Equal("Index",Assert.IsType<RedirectToActionResult>(await Controller().Create(customer)).ActionName);repo.Verify(r=>r.AddAsync(customer),Times.Once);}
 [Fact] public async Task CreateInvalidDoesNotSave(){var c=Controller();c.ModelState.AddModelError("FirstName","Required");var customer=Customer();
 Assert.Same(customer,Assert.IsType<ViewResult>(await c.Create(customer)).Model);repo.VerifyNoOtherCalls();}
 [Fact] public async Task EditValidUpdatesExistingEntity(){var existing=Customer();repo.Setup(r=>r.FindAsync(7)).ReturnsAsync(existing);repo.Setup(r=>r.SaveAsync()).Returns(Task.CompletedTask);
 var input=new Customer {FirstName="Olena",LastName="Koval",Address="Kyiv",Discount=10};
 Assert.IsType<RedirectToActionResult>(await Controller().Edit(7,input));Assert.Equal("Olena",existing.FirstName);Assert.Equal("Koval",existing.LastName);Assert.Equal("Kyiv",existing.Address);Assert.Equal(10,existing.Discount);repo.VerifyAll();}
 [Fact] public async Task EditInvalidDoesNotChangeOrSave(){var existing=Customer();repo.Setup(r=>r.FindAsync(7)).ReturnsAsync(existing);var c=Controller();c.ModelState.AddModelError("FirstName","Required");
 Assert.IsType<ViewResult>(await c.Edit(7,new Customer()));Assert.Equal("Ivan",existing.FirstName);repo.Verify(r=>r.SaveAsync(),Times.Never);}
 [Fact] public async Task EditMissingReturnsNotFound(){repo.Setup(r=>r.FindAsync(7)).ReturnsAsync((Customer)null);Assert.IsType<NotFoundResult>(await Controller().Edit(7,Customer()));repo.VerifyAll();}
 [Fact] public async Task DeleteMissingReturnsNotFound(){repo.Setup(r=>r.FindAsync(7)).ReturnsAsync((Customer)null);Assert.IsType<NotFoundResult>(await Controller().DeleteConfirmed(7));repo.VerifyAll();}
 [Fact] public async Task DeleteWithOrdersReturnsErrorWithoutDeleting(){var customer=Customer();repo.Setup(r=>r.FindAsync(7)).ReturnsAsync(customer);repo.Setup(r=>r.HasOrdersAsync(7)).ReturnsAsync(true);
 var c=Controller();var result=Assert.IsType<ViewResult>(await c.DeleteConfirmed(7));Assert.Equal("Delete",result.ViewName);Assert.False(c.ModelState.IsValid);repo.Verify(r=>r.DeleteAsync(It.IsAny<Customer>()),Times.Never);}
 [Fact] public async Task DeleteWithoutOrdersDeletesAndRedirects(){var customer=Customer();repo.Setup(r=>r.FindAsync(7)).ReturnsAsync(customer);repo.Setup(r=>r.HasOrdersAsync(7)).ReturnsAsync(false);repo.Setup(r=>r.DeleteAsync(customer)).Returns(Task.CompletedTask);
 Assert.Equal("Index",Assert.IsType<RedirectToActionResult>(await Controller().DeleteConfirmed(7)).ActionName);repo.VerifyAll();}
}
}