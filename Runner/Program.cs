using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Runner
{
   class Program
   {
      static void Main(string[] args)
      {
         RunSagaManager();
         RunApp();
      }

      private static void RunApp()
      {
         PrintMenu();
         while (true)
         {
            var msg = Console.ReadKey();
            switch (msg.Key)
            {
               case ConsoleKey.Escape:
                  return;
               case ConsoleKey.D:
                  Console.WriteLine(Persistence.Dump());
                  break;
               case ConsoleKey.S:
                  DumpToFile();
                  break;
               case ConsoleKey.T:
                  SetTasksDelay();
                  break;
               case ConsoleKey.R:
                  RunScenario();
                  break;
               default:
                  PrintMenu();
                  break;
            }
         }
      }

      private static void DumpToFile()
      {
         Console.WriteLine("Give file path");
         File.WriteAllText(Console.ReadLine(), Persistence.Dump());
      }

      private static void PrintMenu()
      {
         Console.WriteLine("\nHello," +
                           "\n ESC - quit" +
                           "\n R - run scenarios" +
                           "\n D - print database" +
                           "\n S - print database to file" +
                           "\n T - set tasks delay");
      }

      private static async void RunSagaManager()
      {
         var selling = new SagaConfiguration();
         selling.AddAction("order", "CheckedEvent", new BuyingSagaAction());
         selling.AddAction("payment", "PaidEvent", new BuyingSagaAction());
         selling.AddEndAction("delivery","SendEvent");

         await new ServiceSimulator<Saga>(() => new Saga(selling)).Start();
      }

      private static async void RunScenario()
      {
         Console.WriteLine("How many user scenarios should be run ?");
         if (int.TryParse(Console.ReadLine(), out var howMany) && howMany > 0)
         {
            Console.WriteLine($"Running {howMany} user scenarios.");
            await UserScenario.RunRange(howMany);
         }
      }

      private static void SetTasksDelay()
      {
         Console.WriteLine("Type max delay (0-max), or range (min-max) in seconds");
         var range = Console.ReadLine().Split('-');
         var max = 0;
         var min = 0;
         if (range.Length == 2)
         {
            int.TryParse(range[0], out min);
            int.TryParse(range[1], out max);
         }
         else if (range.Length == 1)
            int.TryParse(range[0], out max);

         SagaUtils.MinTaskDelay = min * 1000;
         SagaUtils.MaxTaskDelay = max * 1000;
      }
   }
}
