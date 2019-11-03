using System;
using System.Threading.Tasks;
using Domain.Order;
using Domain.Dispatch;
using Persistence;
using Common.General;

namespace App
{
   public class Dispatch
   {
      IPersistenceClient persistence;

      public Dispatch(IPersistenceClient persistence)
      {
         this.persistence = persistence;
      }

      public async Task DispatchOrder(string orderId, string paymentId, string correlationId)
      {
         var state = new Domain.Dispatch.Dispatch(StreamNumbering.NewStreamId<Domain.Dispatch.Dispatch>());
         var order = await this.persistence.GetState<Domain.Order.Order>(orderId);

         var dispatched = new DispatchOrder(correlationId, DateTime.Now)
         {
            PaymentStreamId = paymentId,
            OrderStreamId = orderId,
            Items = order.Items
         }.Execute(state);

         await this.persistence.Save(dispatched);
      }
   }
}
