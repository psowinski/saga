using System;
using System.Collections.Generic;

namespace Orders
{
   public class AddedToOrderEvent : Event
   {
      public AddedToOrderEvent(string streamId, int version) : base(streamId, version)
      {
         Type = nameof(AddedToOrderEvent);
      }

      public string Details;
      public decimal Price;
   }

   public class CheckoutEvent : Event
   {
      public CheckoutEvent(string streamId, int version) : base(streamId, version)
      {
         Type = nameof(CheckoutEvent);
      }
   }

   public class OrderState : State
   {
      public OrderState(string streamId) : base(streamId) {}

      public List<string> Details = new List<string>();
      public decimal TotalCost = 0;

      private void Apply(AddedToOrderEvent evn)
      {
         base.Apply(evn);
         Details.Add(evn.Details);
         TotalCost += evn.Price;
      }

      public bool Checkout;
      private void Apply(CheckoutEvent evn)
      {
         base.Apply(evn);
         Checkout = true;
      }

      public new void Apply(Event evn)
      {
         switch (evn)
         {
            case AddedToOrderEvent ae:
               Apply(ae);
               break;
            case CheckoutEvent ce:
               Apply(ce);
               break;
         }
      }

      public void Apply(IEnumerable<Event> events)
      {
         foreach (var @event in events)
            Apply(@events);
      }
   }

   public class OrdersAggregate
   {
      public OrderState Zero(string streamId) { return new OrderState(streamId); }

      public List<Event> AddToOrder(OrderState state, string details, decimal price)
      {
         return new List<Event> {new AddedToOrderEvent(state.StreamId, state.Version + 1)
         {
            Details = details,
            Price = price
         }};
      }

      public List<Event> Checkout(OrderState state)
      {
         return new List<Event> {new CheckoutEvent(state.StreamId, state.Version + 1)};
      }
   }
}
