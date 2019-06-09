using System;
using System.ComponentModel;

namespace Orders
{
   [DisplayName("PaidEvent")]
   public class PaidEvent : Event
   {
      public string OrderStreamId = "";
      public decimal Amount = 0.0m;
   }

   public class PaymentState : State
   {
      public string OrderStreamId = "";
      public decimal Amount = 0.0m;
   }

   public class PaymentAggregate
   {
      public PaymentState Zero(string streamId)
      {
         return new PaymentState
         {
            StreamId = streamId
         };
      }

      public PaidEvent Pay(PaymentState state, string orderStreamId, decimal amount)
      {
         return new PaidEvent
         {
            StreamId = state.StreamId,
            Version = state.Version + 1,
            OrderStreamId = orderStreamId,
            Amount = amount
         };
      }

      public PaymentState Apply(PaymentState state, PaidEvent evn)
      {
         if (state.Version + 1 != evn.Version) throw new ArgumentException(nameof(evn.Version));
         if (state.StreamId != evn.StreamId) throw new ArgumentException(nameof(evn.StreamId));

         return new PaymentState
         {
            StreamId = evn.StreamId,
            Version = evn.Version + 1,
            OrderStreamId = evn.OrderStreamId,
            Amount = evn.Amount
         };
      }
   }
}
