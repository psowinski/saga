using System.Collections.Generic;
using System.ComponentModel;
using Domain.Common;

namespace Domain.Orders
{
   public class OrderState : State
   {
      public OrderState(string streamId) : base(streamId)
      {
      }

      private readonly List<string> items = new List<string>();
      public IReadOnlyList<string> Items => this.items;
      public decimal TotalCost { get; private set; }
      public bool CheckedOut { get; private set; }

      public void Apply(ItemAddedEvent evn)
      {
         base.ApplyVersion(evn);
         this.items.Add(evn.Description);
         TotalCost += evn.Cost;
      }

      public void Apply(CheckedOutEvent evn)
      {
         base.ApplyVersion(evn);
         CheckedOut = true;
      }

      public override void Apply(Event evn)
      {
         switch (evn)
         {
            case ItemAddedEvent x: Apply(x); break;
            case CheckedOutEvent x: Apply(x); break;
            default: throw new InvalidEnumArgumentException(nameof(evn));
         }
      }
   }
}