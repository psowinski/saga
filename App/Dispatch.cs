﻿using System;
using System.Threading.Tasks;
using Common.Aggregate;
using DomainOrder = Domain.Order.Order;
using DomainDispatch = Domain.Dispatch.Dispatch;
using Domain.Dispatch;
using Persistence;
using Common.General;

namespace App
{
   public class Dispatch
   {
      private readonly IPersistenceClient persistence;

      public Dispatch(IPersistenceClient persistence)
      {
         this.persistence = persistence;
      }

      public async Task DispatchOrder(string orderId, string paymentId, string correlationId)
      {
         var state = new DomainDispatch(StreamNumbering.NewStreamId<DomainDispatch>());
         var order = await this.persistence.GetState<DomainOrder, GeneralUpdater<DomainOrder>>(orderId);

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
