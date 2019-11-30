using Common.Aggregate;

namespace Domain.Payment
{
   public class PaymentCompleted : AggregateEvent<PaymentV2>
   {
      public string OrderStreamId { get; set; }
   }
}