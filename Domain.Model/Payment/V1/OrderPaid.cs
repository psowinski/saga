using Common.Aggregate;

namespace Domain.Model.Payment.V1
{
   //payment ver. 1 replaced in ver. 2 by PaymentRequested & PaymentCompleted
   public class OrderPaid : AggregateEvent<Payment>
   {
      public string OrderStreamId { get; set; }
      public decimal Amount { get; set; }
   }
}