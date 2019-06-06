using System;
using System.Collections.Generic;

namespace Orders
{
   public class OrderPaidEvent : Event
   {
      public OrderPaidEvent(string streamId, int version) : base(streamId, version)
      {
         Type = nameof(OrderPaidEvent);
      }

      public string OrderStreamId;
      public decimal Amount;
   }

   public class PaymentState : State
   {
      public PaymentState(string streamId) : base(streamId) {}

      public string OrderStreamId;
      public decimal Amount;

      public void Apply(OrderPaidEvent evn)
      {
         base.Apply(evn);
         OrderStreamId = evn.OrderStreamId;
         Amount = evn.Amount;
      }
   }

   public class PaymentAggregate
   {
      public PaymentState Zero(string streamId) { return new PaymentState(streamId); }

      public List<Event> Pay(PaymentState state, string orderStreamId, decimal amount)
      {
         return new List<Event> { new OrderPaidEvent(state.StreamId, state.Version + 1)
         {
            OrderStreamId = orderStreamId,
            Amount = amount
         }};
      }
   }
}
