using System;
using System.IO;
using System.Linq;
using Domain.Common;

namespace Domain.Orders
{
   public class CheckOutCommand : Command
   {
      public CheckOutCommand(string correlationId, DateTime timeStamp) : base(correlationId, timeStamp)
      {
      }

      private void Validate(OrderState state)
      {
         if (state.CheckedOut) throw new ArgumentException(nameof(state.CheckedOut));
         if (!state.Items.Any()) throw new InvalidDataException(nameof(state.Items));
      }

      public CheckedOutEvent Execute(OrderState state)
      {
         Validate(state);
         return new CheckedOutEvent(state, this);
      }
   }
}