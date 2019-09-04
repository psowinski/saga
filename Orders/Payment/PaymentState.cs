using System.ComponentModel;
using Domain.Common;

namespace Domain.Payment
{
   public class PaymentState : State
   {
      public PaymentState(string streamId) : base(streamId)
      {
      }

      public string OrderStreamId { get; private set; }
      public decimal Amount { get; private set; }

      public void Apply(PaidEvent evn)
      {
         base.ApplyVersion(evn);

         OrderStreamId = evn.OrderStreamId;
         Amount = evn.Amount;
      }

      public override void Apply(Event evn)
      {
         switch (evn)
         {
            case PaidEvent x: Apply(x); break;
            default: throw new InvalidEnumArgumentException(nameof(evn));
         }
      }
   }
}
