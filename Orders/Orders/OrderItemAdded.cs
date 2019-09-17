using Domain.Common;

namespace Domain.Orders
{
   public class OrderItemAdded : SagaEvent<Order>
   {
      public OrderItemAdded()
      {
      }

      public OrderItemAdded(Order state, AddOrderItem command) : base(state, command)
      {
         Description = command.Description;
         Cost = command.Cost;
      }

      public string Description { get; set; }
      public decimal Cost { get; set; }
   }
}