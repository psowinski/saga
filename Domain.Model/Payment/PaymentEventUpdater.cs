using Common.Aggregate;
using System;
using System.Collections.Generic;

namespace Domain.Payment
{
   public class PaymentEventUpdater : IEventUpdater<PaymentV2>
   {
      public IEnumerable<AggregateEvent<PaymentV2>> Update(Event evn)
      {
         return evn switch
         {
            OrderPaid x => Update(x),
            _ => throw new ArgumentException(nameof(evn))
         };
      }

      public IEnumerable<AggregateEvent<PaymentV2>> Update(OrderPaid evn)
      {
         var paymentRequested = evn.CopyTo<PaymentRequested>();
         paymentRequested.OrderStreamId = evn.OrderStreamId;
         paymentRequested.Total = evn.Amount;
         paymentRequested.Description = "";
         yield return paymentRequested;

         yield return evn.CopyTo<PaymentCompleted>();
      }
   }
}
