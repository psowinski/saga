using System.Collections.Generic;
using System.ComponentModel;
using Common.Aggregate;

namespace Domain.Dispatch
{
   public class Dispatch : State
   {
      public Dispatch(string streamId) : base(streamId)
      {
      }

      public string PaymentStreamId { get; private set; }
      public string OrderStreamId { get; private set; }
      private readonly List<string> items = new List<string>();
      public IReadOnlyList<string> Items => this.items;

      public void Apply(OrderDispatched evn)
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
            case OrderDispatched x: Apply(x); break;
            default: throw new InvalidEnumArgumentException(nameof(evn));
         }
      }
   }
}