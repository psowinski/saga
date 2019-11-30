using Common.Aggregate;

namespace Domain.Model.Payment
{
   public class PaymentRequested : AggregateEvent<Payment>
   {
      public string OrderStreamId { get; set; }
      public decimal Total { get; set; }
      public string Description { get; set; }
   }
}