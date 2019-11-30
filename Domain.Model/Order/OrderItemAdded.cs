using Common.Aggregate;

namespace Domain.Model.Order
{
   public class OrderItemAdded : AggregateEvent<Order>
   {
      public string Description { get; set; }
      public decimal Cost { get; set; }
   }
}