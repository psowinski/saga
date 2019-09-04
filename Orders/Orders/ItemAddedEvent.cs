using Domain.Common;

namespace Domain.Orders
{
   public class ItemAddedEvent : Event
   {
      public ItemAddedEvent(OrderState state, AddItemCommand command) : base(state, command)
      {
         Description = command.Description;
         Cost = command.Cost;
      }

      public string Description { get; }
      public decimal Cost { get; }
   }
}