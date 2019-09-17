using System;
using System.IO;
using Domain.Common;

namespace Domain.Orders
{
   public class AddOrderItem : Command
   {
      public AddOrderItem(string correlationId, DateTime timeStamp) : base(correlationId, timeStamp)
      {
      }

      public string Description;
      public decimal Cost;

      private void Validate(Order state)
      {
         if (string.IsNullOrWhiteSpace(Description)) throw new InvalidDataException(nameof(Description));
         if (state.CheckedOut) throw new ArgumentException(nameof(state.CheckedOut));
      }

      public OrderItemAdded Execute(Order state)
      {
         Validate(state);
         return new OrderItemAdded(state, this);
      }
   }
}