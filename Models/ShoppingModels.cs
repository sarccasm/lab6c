using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace ProductsValidation.Models {
public class Customer {
 public int Id {get;set;}
 [Required, StringLength(50)] public string FirstName {get;set;}
 [Required, StringLength(50)] public string LastName {get;set;}
 [Required, StringLength(50)] public string Address {get;set;}
 [Range(0,100)] public int Discount {get;set;}
 public string FullName => FirstName + " " + LastName;
 public ICollection<Order> Orders {get;set;} = new List<Order>();
}
public class Supermarket {
 public int Id {get;set;}
 [Required, StringLength(50)] public string Name {get;set;}
 [Required, StringLength(100)] public string Address {get;set;}
 public ICollection<Order> Orders {get;set;} = new List<Order>();
}
public class Order {
 public int Id {get;set;}
 public int CustomerId {get;set;}
 public Customer Customer {get;set;}
 public int SupermarketId {get;set;}
 public Supermarket Supermarket {get;set;}
 public DateTime OrderDate {get;set;} = DateTime.Today;
 public ICollection<OrderDetails> OrderDetails {get;set;} = new List<OrderDetails>();
}
public class OrderDetails {
 public int Id {get;set;}
 public int OrderId {get;set;}
 public Order Order {get;set;}
 public int ProductId {get;set;}
 public Product Product {get;set;}
 [Range(typeof(decimal), "0.001", "100000")] public decimal Quantity {get;set;}
 public decimal UnitPrice {get;set;}
}
public class OrderForm {
 public int Id {get;set;}
 [Range(1,int.MaxValue)] public int CustomerId {get;set;}
 [Range(1,int.MaxValue)] public int SupermarketId {get;set;}
 [Required] public DateTime OrderDate {get;set;} = DateTime.Today;
 public List<OrderLineForm> Lines {get;set;} = new List<OrderLineForm>();
}
public class OrderLineForm {
 public int ProductId {get;set;}
 public string Name {get;set;}
 public decimal Price {get;set;}
 [Range(typeof(decimal), "0", "100000")] public decimal Quantity {get;set;}
}
}