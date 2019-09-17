using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Orders;
using Infrastructure;
using Sagas.Common;

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
         var order = new Order(StreamNumbering.NewStreamId<Order>());

         //Command 1: Add first item
         await Delayer.WaitSomeTime();
         var addMilk = new AddOrderItem(StreamNumbering.NewCorrelationId(), DateTime.Now)
         {
            Description = "Milk",
            Cost = 3.0m
         };
         Console.WriteLine($"[{addMilk.CorrelationId}/{order.StreamId}] Adding milk - 3.00");

         var milkAdded = addMilk.Execute(order);
         await bus.Pipe(milkAdded);

         //Command 2: Add second item
         await Delayer.WaitSomeTime();
         var addBread = new AddOrderItem(StreamNumbering.NewCorrelationId(), DateTime.Now)
         {
            Description = "Bread",
            Cost = 5.0m
         };
         Console.WriteLine($"[{addBread.CorrelationId}/{order.StreamId}] Adding bread - 5.00");

         order.Apply(milkAdded);
         var breadAdded = addBread.Execute(order);
         await bus.Pipe(breadAdded);

         //Command 3: Checkout
         await Delayer.WaitSomeTime();
         var checkOut = new CheckOutOrder(StreamNumbering.NewCorrelationId(), DateTime.Now);
         Console.WriteLine($"[{checkOut.CorrelationId}/{order.StreamId}] Checkout");

         order.Apply(breadAdded);
         await bus.Pipe(checkOut.Execute(order));
      }
   }
}