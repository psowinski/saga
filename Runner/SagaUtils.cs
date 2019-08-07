using System;
using System.Threading.Tasks;

namespace Runner
{
   public static class SagaUtils
   {
      public static Random Rnd = new Random();

      public static async Task WaitSomeTime()
      {
         if(MaxTaskDelay > MinTaskDelay && MinTaskDelay >= 0 && MaxTaskDelay > 0)
            await Task.Delay(SagaUtils.Rnd.Next(MinTaskDelay, MaxTaskDelay));
      }

      public static int MinTaskDelay = 1000;
      public static int MaxTaskDelay = 2000;

      private static readonly object NumberLock = new object();
      private static int LastNumber = 0;

      private static int NextNumber()
      {
         lock (NumberLock)
         {
            return ++LastNumber;
         }
      }

      public static string GenerateOrderId() => $"order-{NextNumber():D5}";
      public static string GeneratePaymentId() => $"payment-{NextNumber():D5}";
      public static string GenerateDeliveryId() => $"delivery-{NextNumber():D5}";

      public static string GenerateCorrelationId() => $"saga-{NextNumber():D5}";
      public static DateTime GenerateTimeStamp() => DateTime.Now;
   }
}