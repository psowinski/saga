using System.Collections.Generic;
using System.ComponentModel;
using Common.Aggregate;

namespace Domain.Order
{
   public class Order : State
   {
      public Order(string streamId) : base(streamId)
      {
      }

      private readonly List<string> items = new List<string>();
      public IReadOnlyList<string> Items => this.items;
      public decimal TotalCost { get; private set; }
      public bool CheckedOut { get; private set; }

      public void Apply(OrderItemAdded evn)
      {
         base.ApplyVersion(evn);
         this.items.Add(evn.Description);
         TotalCost += evn.Cost;
      }

      public void Apply(OrderCheckedOut evn)
      {
         base.ApplyVersion(evn);
         CheckedOut = true;
      }

      public override void Apply(Event evn)
      {
         switch (evn)
         {
            case OrderItemAdded x: Apply(x); break;
            case OrderCheckedOut x: Apply(x); break;
            default: throw new InvalidEnumArgumentException(nameof(evn));
         }
      }
   }
}