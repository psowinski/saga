using System;
using System.ComponentModel;

namespace Domain
{
   public class PayCommand : Command
   {
      public string OrderStreamId;
      public decimal Amount;
   }

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

      public PaidEvent Execute(PaymentState state, PayCommand command)
      {
         return new PaidEvent
         {
            CorrelationId = command.CorrelationId,
            TimeStamp = command.TimeStamp,

            StreamId = state.StreamId,
            Version = state.Version + 1,

            OrderStreamId = command.OrderStreamId,
            Amount = command.Amount
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
