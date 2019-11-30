using Common.General;
using Domain.Order;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace UserScenarioApp
{
   public class ShoppingScenario
   {
      private static AppClient AppClient = new AppClient("http://localhost:9090/");

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
         var orderId = StreamNumbering.NewStreamId<Order>();
         var correlationId = StreamNumbering.NewCorrelationId();

         await Delayer.WaitSomeTime();

         await AppClient.AddItem(orderId, correlationId, "Apple", 3.00m);
         Console.WriteLine($"[{correlationId}/{orderId}] Adding apple - 3.00");

         await Delayer.WaitSomeTime();

         await AppClient.AddItem(orderId, correlationId, "Carrot", 2.50m);
         Console.WriteLine($"[{correlationId}/{orderId}] Adding carrot - 2.50");

         await Delayer.WaitSomeTime();
         await AppClient.Checkout(orderId, correlationId);
         Console.WriteLine($"[{correlationId}/{orderId}] Checkout");
      }
   }
}
