using System;
using Common.Aggregate;

namespace Domain.Payment
{
   public enum PaymentStatus
   {
      Unpaid,
      Pending,
      Completed,
      Cancelled
   }

   public class PaymentV2 : State
   {
      public PaymentV2(string streamId) : base(streamId)
      {
         Status = PaymentStatus.Unpaid;
      }

      public string OrderStreamId { get; private set; }
      public decimal Total { get; private set; }
      public string Description { get; private set; }
      public PaymentStatus Status { get; private set; }

      public void Apply(PaymentRequested evn)
      {
         base.ApplyVersion(evn);

         OrderStreamId = evn.OrderStreamId;
         Total = evn.Total;
         Description = evn.Description;
         Status = PaymentStatus.Pending;
      }

      public void Apply(PaymentCompleted evn)
      {
         base.ApplyVersion(evn);
         Status = PaymentStatus.Completed;
      }
      public void Apply(PaymentCancelled evn)
      {
         base.ApplyVersion(evn);
         Status = PaymentStatus.Cancelled;
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
