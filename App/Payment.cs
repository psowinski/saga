using System;
using System.Threading.Tasks;
using Domain.Order;
using Domain.Payment;
using Persistence;
using Common.General;

namespace App
{
   public class Payment
   {
      IPersistenceClient persistence;

      public Payment(IPersistenceClient persistence)
      {
         this.persistence = persistence;
      }

      public async Task Pay(string orderId, string correlationId)
      {
         var state = new Domain.Payment.Payment(StreamNumbering.NewStreamId<Domain.Payment.Payment>());
         var order = await this.persistence.GetState<Domain.Order.Order>(orderId);

         var payed = new PayForOrder(correlationId, DateTime.Now)
         {
            OrderStreamId = order.StreamId,
            Amount = order.TotalCost
         }.Execute(state);

         await this.persistence.Save(payed);
      }
   }
}
