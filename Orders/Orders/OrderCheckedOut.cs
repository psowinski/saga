using Domain.Common;

namespace Domain.Orders
{
   public class OrderCheckedOut : SagaEvent<Order>
   {
      public OrderCheckedOut()
      {
      }

      public OrderCheckedOut(Order state, CheckOutOrder command) : base(state, command)
      {
      }
   }
}