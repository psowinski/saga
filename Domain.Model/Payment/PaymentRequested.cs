using Common.Aggregate;

namespace Domain.Payment
{
   public class PaymentRequested : AggregateEvent<PaymentV2>
   {
      public string OrderStreamId { get; set; }
      public decimal Total { get; set; }
      public string Description { get; set; }
   }
}