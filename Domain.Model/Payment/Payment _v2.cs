using System;
using Common.Aggregate;

namespace Domain.Payment
{
   public enum PaymentState
   {
      Pending,
      Completed,
      Cancelled
   }

   public class Payment_v2 : State
   {
      public Payment_v2(string streamId) : base(streamId)
      {
         State = PaymentState.Pending;
      }

      public string OrderStreamId { get; private set; }
      public decimal Total { get; private set; }
      public string Description { get; private set; }
      public PaymentState State { get; private set; }

      public void Apply(PaymentRequested evn)
      {
         base.ApplyVersion(evn);

         OrderStreamId = evn.OrderStreamId;
         Total = evn.Total;
         Description = evn.Description;
      }

      public void Apply(PaymentCompleted evn)
      {
         base.ApplyVersion(evn);
         State = PaymentState.Completed;
      }
      public void Apply(PaymentCancelled evn)
      {
         base.ApplyVersion(evn);
         State = PaymentState.Cancelled;
      }

      public override void Apply(Event evn)
      {
         switch (evn)
         {
            case OrderPaid x: throw new MissingUpdateException(x.GetType().FullName);
            case PaymentRequested x: Apply(x); break;
            case PaymentCompleted x: Apply(x); break;
            case PaymentCancelled x: Apply(x); break;
            default: throw new ArgumentException(nameof(evn));
         }
      }
   }
}
