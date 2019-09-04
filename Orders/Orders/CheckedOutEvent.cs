using Domain.Common;

namespace Domain.Orders
{
   public class CheckedOutEvent : Event
   {
      public CheckedOutEvent(OrderState state, CheckOutCommand command) : base(state, command)
      {
      }
   }
}