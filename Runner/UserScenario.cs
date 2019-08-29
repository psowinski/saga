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

         //Command 1: Add first item
         await SagaUtils.WaitSomeTime();
         var addItem1 = new AddItemCommand
         {
            CorrelationId = SagaUtils.GenerateCorrelationId(),
            TimeStamp = SagaUtils.GenerateTimeStamp(),

            Description = "Milk",
            Cost = 3.0m
         };
         Console.WriteLine($"[{addItem1.CorrelationId}/{order.StreamId}] Adding milk - 3.00");

         var item1Added = orders.Execute(order, addItem1);
         await bus.Pipe(item1Added);

         //Command 2: Add second item
         var addItem2 = new AddItemCommand
         {
            CorrelationId = SagaUtils.GenerateCorrelationId(),
            TimeStamp = SagaUtils.GenerateTimeStamp(),

            Description = "Bread",
            Cost = 5.0m
         };
         Console.WriteLine($"[{addItem2.CorrelationId}/{order.StreamId}] Adding bread - 5.00");

         order = orders.Apply(order, item1Added);
         var item2Added = orders.Execute(order, addItem2);
         await bus.Pipe(item2Added);

         //Command 3: Checkout
         await SagaUtils.WaitSomeTime();
         var checkoutCommand = new CheckOutCommand
         {
            CorrelationId = SagaUtils.GenerateCorrelationId(),
            TimeStamp = SagaUtils.GenerateTimeStamp()
         };
         Console.WriteLine($"[{checkoutCommand.CorrelationId}/{order.StreamId}] Checkout");

         order = orders.Apply(order, item2Added);
         await bus.Pipe(orders.Execute(order, checkoutCommand));
      }
   }
}