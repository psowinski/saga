using System;
using System.IO;
using Common.Aggregate;
using Domain.Model.Order;

namespace Domain.BusinessLogic.Order
{
   public class AddOrderItem : Command
   {
      public AddOrderItem(string correlationId, DateTime timeStamp) : base(correlationId, timeStamp)
      {
      }

      public string Description { get; set;}
      public decimal Cost { get; set; }

      private void Validate(Model.Order.Order state)
      {
         if (string.IsNullOrWhiteSpace(Description)) throw new InvalidDataException(nameof(Description));
         if (state.CheckedOut) throw new ArgumentException(nameof(state.CheckedOut));
      }

      public OrderItemAdded Execute(Model.Order.Order state)
      {
         Validate(state);
         return CreateEvent(state);
      }

      private OrderItemAdded CreateEvent(Model.Order.Order state)
      {
         var evn = CreateEvent<OrderItemAdded>(state);
         evn.Description = Description;
         evn.Cost = Cost;
         return evn;
      }
   }
}