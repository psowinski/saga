using System;
using System.IO;
using System.Linq;
using Common.Aggregate;
using Domain.Model.Order;

namespace Domain.BusinessLogic.Order
{
   public class CheckOutOrder : Command
   {
      public CheckOutOrder(string correlationId, DateTime timeStamp) : base(correlationId, timeStamp)
      {
      }

      private void Validate(Model.Order.Order state)
      {
         if (state.CheckedOut) throw new ArgumentException(nameof(state.CheckedOut));
         if (!state.Items.Any()) throw new InvalidDataException(nameof(state.Items));
      }

      public OrderCheckedOut Execute(Model.Order.Order state)
      {
         Validate(state);
         return CreateEvent<OrderCheckedOut>(state);
      }
   }
}