using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Orders;
using Persistence;

namespace Runner
{
   public static class Bus
   {
      public static Database db = new Database();

      public static void Pipe(List<Event> events)
      {
         foreach (var evn in events)
         {
            db.Save(JsonConvert.SerializeObject(evn));
         }
      }

      public static List<Event> DeserializeOrderEvents(string json)
      {
         return null;
      }
   }

   public class BuyingSaga
   {
      void OnCheckout(CheckoutEvent evn)
      {
         Console.WriteLine("Read order");
         var orders = new OrdersAggregate();
         var myOrder = orders.Zero(evn.StreamId);
         myOrder.Apply(Bus.DeserializeOrderEvents(Bus.db.Load(evn.StreamId)));

         Console.WriteLine("Pay");
         var payment = new PaymentAggregate();
         var myPayment = payment.Zero("payment-1");
         Bus.Pipe(payment.Pay(myPayment, evn.StreamId, myOrder.TotalCost));
      }
   }

   class Program
   {
      static void Main(string[] args)
      {
         var orders = new OrdersAggregate();
         var myOrder = orders.Zero("orders-1");
         Console.WriteLine("Adding milk - 3.00");
         Bus.Pipe(orders.AddToOrder(myOrder, "Milk", 3.0m));
         Console.WriteLine("Adding bread - 5.00");
         Bus.Pipe(orders.AddToOrder(myOrder, "Bread", 5.0m));
         Console.WriteLine("Checkout");
         Bus.Pipe(orders.Checkout(myOrder));




         //var jsonRead = db.Load("a1");
         //var objRead = JsonConvert.DeserializeObject<List<AddToOrder>>(jsonRead);
      }
   }
}
