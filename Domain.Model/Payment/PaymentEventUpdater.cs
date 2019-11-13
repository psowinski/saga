using Common.Aggregate;
using System;
using System.Collections.Generic;

namespace Domain.Payment
{
   public class PaymentEventUpdater : EventUpdater<Payment_v1>
   {
      public override IEnumerable<AggregateEvent<Payment_v1>> Update(AggregateEvent<Payment_v1> evn)
      {
         switch (evn)
         {
            case OrderPaid x: return Update(x);
            default: throw new ArgumentException(nameof(evn));
         }
      }

      public IEnumerable<AggregateEvent<Payment_v1>> Update(OrderPaid evn)
      {
         var paymentRequested = CopyEvent<PaymentRequested>(evn);
         paymentRequested.OrderStreamId = evn.OrderStreamId;
         paymentRequested.Total = evn.Amount;
         paymentRequested.Description = "";
         yield return paymentRequested;

         yield return CopyEvent<PaymentCompleted>(evn);
      }
   }
}
