using System.Collections.Generic;
using System.Linq;

namespace Orders
{
   public class DeliveryState : State
   {
      public DeliveryState(string streamId) : base(streamId)
      {
      }

      public string OrderStreamId;
      public List<string> Details = new List<string>();

      public void Apply(OrderSentEvent evn)
      {
         base.Apply(evn);
         OrderStreamId = evn.OrderStreamId;
         Details.AddRange(evn.Details);
      }
   }

   public class OrderSentEvent : Event
   {
      public OrderSentEvent(string streamId, int version) : base(streamId, version)
      {
      }

      public string OrderStreamId;
      public List<string> Details;
   }

   public class DeliveryAggregate
   {
      public DeliveryState Zero(string streamId) { return new DeliveryState(streamId); }

      public List<Event> Send(DeliveryState state, string orderStreamId, IEnumerable<string> details)
      {
         return new List<Event> { new OrderSentEvent(state.StreamId, state.Version + 1)
         {
            OrderStreamId = orderStreamId,
            Details = details.ToList()
         }};
      }
   }
}
