using System;

namespace UserScenarioApp
{
   class Program
   {
      static void Main(string[] args)
      {
         PrintMenu();
         while (true)
         {
            var msg = Console.ReadKey();
            Console.WriteLine();
            switch (msg.Key)
            {
               case ConsoleKey.Escape:
                  return;
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

      private static void PrintMenu()
      {
         Console.WriteLine("\nHello," +
                           "\n ESC - quit" +
                           "\n R - run shopping scenarios" +
                           "\n T - set tasks delay");
      }

      private static async void RunScenario()
      {
         Console.WriteLine("How many user scenarios should be run ?");
         if (int.TryParse(Console.ReadLine(), out var howMany) && howMany > 0)
         {
            Console.WriteLine($"Running {howMany} user scenarios.");
            await ShoppingScenario.RunRange(howMany);
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

         Delayer.MinTaskDelay = min * 1000;
         Delayer.MaxTaskDelay = max * 1000;
      }
   }
}
