using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Aggregate;
using Domain.Order;
using Domain.Payment;
using Persistence;
using Common.General;
using DomainOrder = Domain.Order.Order;

namespace App
{
   public class Payment
   {
      IPersistenceClient persistence;

      public Payment(IPersistenceClient persistence)
      {
         this.persistence = persistence;
      }

      public async Task Pay_v1(string orderId, string correlationId)
      {
         var state = new Payment_v1(StreamNumbering.NewStreamId<Payment_v1>());
         var order = await this.persistence.GetState<DomainOrder, EventUpdater<DomainOrder>>(orderId);

         var payed = new PayForOrder_v1(correlationId, DateTime.Now)
         {
            OrderStreamId = order.StreamId,
            Amount = order.TotalCost
         }.Execute(state);

         await this.persistence.Save(payed);
      }

      public async Task Pay_v2(string orderId, string correlationId)
      {
         var state = new Payment_v2(StreamNumbering.NewStreamId<Payment_v2>());
         var order = await this.persistence.GetState<DomainOrder, EventUpdater<DomainOrder>>(orderId);

         var payed = new PayForOrder_v2(correlationId, DateTime.Now)
         {
            OrderStreamId = order.StreamId,
            Total = order.TotalCost,
            Description = order.Items.Aggregate((acc, x) => acc + ',' + x)
         }.Execute(state);

         await this.persistence.Save(payed);
      }
   }
}
