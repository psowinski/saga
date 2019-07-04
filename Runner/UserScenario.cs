using System;
using System.Linq;
using System.Threading.Tasks;
using Domain;

namespace Runner
{
   static class UserScenario
   {
      public static async Task RunRange(int count)
      {
         try
         {
            var tasks = Enumerable.Range(0, count).Select(idx => Run()).ToList();
            await Task.WhenAll(tasks);
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
         }
      }

      public static async Task Run()
      {
         var rnd = new Random();

         var bus = new Bus();
         var orders = new OrdersAggregate();
         var order = orders.Zero(SagaUtils.GenerateOrderId());

         await SagaUtils.DelayedHeader($"[{order.StreamId}] Adding milk - 3.00");

         var evnAdded1 = orders.AddItem(order, "Milk", 3.0m);
         await bus.Pipe(evnAdded1);

         await SagaUtils.DelayedHeader($"[{order.StreamId}] Adding bread - 5.00");

         order = orders.Apply(order, evnAdded1);
         var evnAdded2 = orders.AddItem(order, "Bread", 5.0m);
         await bus.Pipe(evnAdded2);

         await SagaUtils.DelayedHeader($"[{order.StreamId}] Checkout");

         order = orders.Apply(order, evnAdded2);
         await bus.Pipe(orders.Checkout(order));
      }
   }
}