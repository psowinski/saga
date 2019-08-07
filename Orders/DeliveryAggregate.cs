using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Domain
{
   public class SendCommand : Command
   {
      public string PaymentStreamId;
      public string OrderStreamId;
      public IEnumerable<string> Items;
   }

   [DisplayName("SendEvent")]
   public class SendEvent : Event
   {
      public string PaymentStreamId = "";
      public string OrderStreamId = "";
      public List<string> Items = new List<string>();
   }

   public class DeliveryState : State
   {
      public string PaymentStreamId = "";
      public string OrderStreamId = "";
      public List<string> Items = new List<string>();
   }

   public class DeliveryAggregate
   {
      public DeliveryState Zero(string streamId)
      {
         return new DeliveryState
         {
            StreamId = streamId
         };
      }

      public SendEvent Send(DeliveryState state, SendCommand command)
      {
         return new SendEvent
         {
            CorrelationId = command.CorrelationId,
            TimeStamp = command.TimeStamp,

            StreamId = state.StreamId,
            Version = state.Version + 1,

            OrderStreamId = command.OrderStreamId,
            PaymentStreamId = command.PaymentStreamId,
            Items = command.Items.ToList()
         };
      }

      public DeliveryState Apply(DeliveryState state, SendEvent evn)
      {
         if (state.Version + 1 != evn.Version) throw new ArgumentException(nameof(evn.Version));
         if (state.StreamId != evn.StreamId) throw new ArgumentException(nameof(evn.StreamId));

         return new DeliveryState
         {
            StreamId = evn.StreamId,
            Version = evn.Version,
            PaymentStreamId = evn.PaymentStreamId,
            OrderStreamId = evn.OrderStreamId,
            Items = evn.Items.ToList()
         };
      }
   }
}
