using System;
using System.Threading.Tasks;

namespace Infrastructure
{
   public static class Delayer
   {
      public static Random Rnd = new Random();

      public static async Task WaitSomeTime()
      {
         if (MaxTaskDelay > MinTaskDelay && MinTaskDelay >= 0 && MaxTaskDelay > 0)
         {
            var timeSpan = Rnd.Next(MinTaskDelay, MaxTaskDelay);
            await Task.Delay(timeSpan);
         }
      }

      public static int MinTaskDelay = 1000;
      public static int MaxTaskDelay = 5000;
   }
}