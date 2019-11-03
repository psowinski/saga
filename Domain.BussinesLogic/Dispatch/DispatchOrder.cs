using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Aggregate;

namespace Domain.Dispatch
{
   public class DispatchOrder : Command
   {
      public DispatchOrder(string correlationId, DateTime timeStamp) : base(correlationId, timeStamp)
      {
      }

      public string PaymentStreamId;
      public string OrderStreamId;
      public IEnumerable<string> Items;

      private void Validate()
      {
         if (string.IsNullOrWhiteSpace(PaymentStreamId)) throw new InvalidDataException(nameof(PaymentStreamId));
         if (string.IsNullOrWhiteSpace(OrderStreamId)) throw new InvalidDataException(nameof(OrderStreamId));
         if (!Items.Any()) throw new InvalidDataException(nameof(Items));
      }

      public OrderDispatched Execute(Dispatch state)
      {
         Validate();
         return CreateEvent(state);
      }

      private OrderDispatched CreateEvent(Dispatch state)
      {
         var evn = CreateEvent<OrderDispatched>(state);
         evn.PaymentStreamId = PaymentStreamId;
         evn.OrderStreamId = OrderStreamId;
         evn.Items = Items.ToList();
         return evn;
      }
   }
}