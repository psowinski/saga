using System;
using System.Threading.Tasks;

namespace Runner
{
   public static class Saga
   {
      public static Random Rnd = new Random();
      public static async Task DelayedHeader(string msg)
      {
         await Task.Delay(Saga.Rnd.Next(1000, 10000));
         Console.WriteLine(msg);
      }

      public static string GenerateOrderId() => $"order-{Rnd.Next(1, 1000)}";
      public static string GeneratePaymentId() => $"payment-{Rnd.Next(1, 1000)}";
      public static string GenerateDeliveryId() => $"delivery-{Rnd.Next(1, 1000)}";
   }
}