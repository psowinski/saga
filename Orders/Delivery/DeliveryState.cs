using System.Collections.Generic;
using System.ComponentModel;
using Domain.Common;

namespace Domain.Delivery
{
   public class DeliveryState : State
   {
      public DeliveryState(string streamId) : base(streamId)
      {
      }

      public string PaymentStreamId { get; private set; }
      public string OrderStreamId { get; private set; }
      private readonly List<string> items = new List<string>();
      public IReadOnlyList<string> Items => this.items;

      public void Apply(SendEvent evn)
      {
         base.ApplyVersion(evn);

         PaymentStreamId = evn.PaymentStreamId;
         OrderStreamId = evn.OrderStreamId;
         this.items.AddRange(evn.Items);
      }

      public override void Apply(Event evn)
      {
         switch (evn)
         {
            case SendEvent x: Apply(x); break;
            default: throw new InvalidEnumArgumentException(nameof(evn));
         }
      }
   }
}