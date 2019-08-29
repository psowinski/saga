using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Domain
{
   public class AddItemCommand : Command
   {
      public string Description;
      public decimal Cost;
   }

   public class CheckOutCommand : Command
   {
   }

   [DisplayName("ItemAddedEvent")]
   public class ItemAddedEvent : Event
   {
      public string Description = "";
      public decimal Cost = 0.0m;
   }

   [DisplayName("CheckedEvent")]
   public class CheckedOutEvent : Event
   {
   }

   public class OrderState : State
   {
      public List<string> Items = new List<string>();
      public decimal TotalCost = 0.0m;
      public bool CheckedOut = false;
   }

   public class OrdersAggregate : IAggregate<OrderState>
   {
      public OrderState Zero(string streamId)
      {
         return new OrderState
         {
            StreamId = streamId,
         };
      }

      public Event Execute(OrderState state, Command command)
      {
         switch (command)
         {
            case AddItemCommand addItem:
               return AddItem(state, addItem);
            case CheckOutCommand checkout:
               return Checkout(state, checkout);
            default:
               throw new NotImplementedException(nameof(command));
         }
      }

      private ItemAddedEvent AddItem(OrderState state, AddItemCommand command)
      {
         return new ItemAddedEvent
         {
            CorrelationId = command.CorrelationId,
            TimeStamp = command.TimeStamp,

            StreamId = state.StreamId,
            Version = state.Version + 1,

            Description = command.Description,
            Cost = command.Cost
         };
      }

      private CheckedOutEvent Checkout(OrderState state, CheckOutCommand command)
      {
         return new CheckedOutEvent
         {
            CorrelationId = command.CorrelationId,
            TimeStamp = command.TimeStamp,

            StreamId = state.StreamId,
            Version = state.Version + 1
         };
      }

      public OrderState Apply(OrderState state, Event evn)
      {
         if (state.Version + 1 != evn.Version) throw new ArgumentException(nameof(evn.Version));
         if (state.StreamId != evn.StreamId) throw new ArgumentException(nameof(evn.StreamId));

         switch (evn)
         {
            case ItemAddedEvent itemAdded:
               if (state.CheckedOut) throw new ArgumentException(nameof(state));
               return new OrderState
               {
                  StreamId = evn.StreamId,
                  Version = evn.Version,
                  Items = state.Items.Append(itemAdded.Description).ToList(),
                  TotalCost = state.TotalCost + itemAdded.Cost
               };
            case CheckedOutEvent checkedOutEvent:
               return new OrderState
               {
                  StreamId = evn.StreamId,
                  Version = evn.Version,
                  Items = state.Items.ToList(),
                  TotalCost = state.TotalCost,
                  CheckedOut = true
               };
         }
         throw new Exception(nameof(evn));
      }
   }
}
