using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Runner
{
   class Program
   {
      static async Task Main(string[] args)
      {
         await UserScenario.RunRange(3);
         Console.ReadKey();
      }
   }
}
