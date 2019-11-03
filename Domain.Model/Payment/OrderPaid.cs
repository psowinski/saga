using Common.Aggregate;

namespace Domain.Payment
{
   public class OrderPaid : AggregateEvent<Payment>
   {
      public string OrderStreamId { get; set; }
      public decimal Amount { get; set; }
   }
}