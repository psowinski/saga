using System.Collections.Generic;
using Common.Aggregate;
using Domain.Model.Payment.V1;

namespace Domain.Model.Payment
{
   public class PaymentEventUpdater : IEventUpdater
   {
      public IEnumerable<Event> Update(Event evn)
      {
         return evn switch
         {
            OrderPaid x => Update(x),
            _ => new[] { evn }
         };
      }

      public IEnumerable<Event> Update(OrderPaid evn)
      {
         var paymentRequested = evn.CopyTo<PaymentRequested>();
         paymentRequested.OrderStreamId = evn.OrderStreamId;
         paymentRequested.Total = evn.Amount;
         paymentRequested.Description = "";
         yield return paymentRequested;

         var paymentCompleted = evn.CopyTo<PaymentCompleted>();
         paymentCompleted.OrderStreamId = evn.OrderStreamId;
         yield return paymentCompleted;
      }
   }
}
