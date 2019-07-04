using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Runner
{
   class Program
   {
      static async Task Main(string[] args)
      {
         Console.WriteLine("Hello, type number of scenarios to run or q to quit.");

         var sm = new SagaManager();
         sm.RegisterSaga("order", "CheckedEvent", new BuyingSaga());
         sm.RegisterSaga("payment", "PaidEvent", new BuyingSaga());
         sm.Run();

         while (true)
         {
            var msg = Console.ReadLine();
            if (msg == "q") return;
            if (msg == "d") Console.WriteLine(Persistence.Dump());
            if (int.TryParse(msg, out var times) && times > 0)
            {
               Console.WriteLine($"Running {times} user scenarios.");
               UserScenario.RunRange(times);
            }
         }
      }
   }
}
