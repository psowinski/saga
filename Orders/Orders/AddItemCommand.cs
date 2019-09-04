using System;
using System.IO;
using Domain.Common;

namespace Domain.Orders
{
   public class AddItemCommand : Command
   {
      public AddItemCommand(string correlationId, DateTime timeStamp) : base(correlationId, timeStamp)
      {
      }

      public string Description;
      public decimal Cost;

      private void Validate(OrderState state)
      {
         if (string.IsNullOrWhiteSpace(Description)) throw new InvalidDataException(nameof(Description));
         if (state.CheckedOut) throw new ArgumentException(nameof(state.CheckedOut));
      }

      public ItemAddedEvent Execute(OrderState state)
      {
         Validate(state);
         return new ItemAddedEvent(state, this);
      }
   }
}