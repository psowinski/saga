using Common.Aggregate;

namespace Domain.Model.Payment
{
   public class PaymentCompleted : AggregateEvent<Payment>
   {
      public string OrderStreamId { get; set; }
   }
}