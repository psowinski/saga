using System;
using Common.Aggregate;

namespace Domain.Payment
{
   public class Payment_v1 : State
   {
      public Payment_v1(string streamId) : base(streamId)
      {
      }

      public string OrderStreamId { get; private set; }
      public decimal Amount { get; private set; }

      public void Apply(OrderPaid evn)
      {
         base.ApplyVersion(evn);

         OrderStreamId = evn.OrderStreamId;
         Amount = evn.Amount;
      }

      public override void Apply(Event evn)
      {
         switch (evn)
         {
            case OrderPaid x: Apply(x); break;
            default: throw new ArgumentException(nameof(evn));
         }
      }
   }
}
