namespace Sagas.Common
{
   public static class NamesGenerator
   {
      private static readonly object NumberLock = new object();
      private static int LastNumber = 0;

      private static int NextNumber()
      {
         lock (NumberLock)
         {
            return ++LastNumber;
         }
      }

      public static string NewOrderId() => $"order-{NextNumber():D5}";
      public static string GeneratePaymentId() => $"payment-{NextNumber():D5}";
      public static string GenerateDeliveryId() => $"delivery-{NextNumber():D5}";

      public static string GenerateCorrelationId() => $"saga-{NextNumber():D5}";
   }
}