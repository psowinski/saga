using System;
using Orders;

namespace Runner
{
   class Program
   {
      static void Main(string[] args)
      {
         var bus = new Bus();

         var orders = new OrdersAggregate();
         var myOrder = orders.Zero("orders-1");
         Console.WriteLine("Adding milk - 3.00");
         bus.Pipe(orders.AddItem(myOrder, "Milk", 3.0m));
         Console.WriteLine("Adding bread - 5.00");
         bus.Pipe(orders.AddItem(myOrder, "Bread", 5.0m));
         Console.WriteLine("Checkout");
         bus.Pipe(orders.Checkout(myOrder));

         Console.ReadKey();
      }
   }
}
