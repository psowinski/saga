using System;
using System.IO;
using System.Linq;
using Common.Aggregate;

namespace Domain.Order
{
   public class CheckOutOrder : Command
   {
      public CheckOutOrder(string correlationId, DateTime timeStamp) : base(correlationId, timeStamp)
      {
      }

      private void Validate(Order state)
      {
         if (state.CheckedOut) throw new ArgumentException(nameof(state.CheckedOut));
         if (!state.Items.Any()) throw new InvalidDataException(nameof(state.Items));
      }

      public OrderCheckedOut Execute(Order state)
      {
         Validate(state);
         return CreateEvent<OrderCheckedOut>(state);
      }
   }
}