using Common.Aggregate;

namespace Domain.Payment
{
   //payment ver. 1 replaced in ver. 2 by PaymentRequested & PaymentCompleted
   public class OrderPaid : AggregateEvent<PaymentV1>
   {
      public string OrderStreamId { get; set; }
      public decimal Amount { get; set; }
   }
}