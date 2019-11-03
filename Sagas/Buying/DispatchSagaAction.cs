using System;
using System.Threading.Tasks;
using Domain.Common;
using Domain.Dispatch;
using Domain.Orders;
using Domain.Payment;
using Infrastructure;
using Sagas.Common;

namespace Sagas.Buying
{
   public class DispatchSagaAction : ISagaAction
   {
      private readonly Bus bus = new Bus();
      private readonly Persistence persistence = new Persistence();

      public async Task ProcessEvent(Event evn)
      {
         if (evn is OrderPaid paid)
            await Dispatch(paid);
         else
            throw new ArgumentException(nameof(evn));
      }

      private async Task Dispatch(OrderPaid evn)
      {
         var state = new Dispatch(StreamNumbering.NewStreamId<Dispatch>());

         var order = await this.persistence.GetState<Order>(evn.OrderStreamId);

         //await Delayer.WaitSomeTime();
         Console.WriteLine($"[{evn.CorrelationId}/{order.StreamId}] Dispatch [{state.StreamId}]");

         await this.bus.Pipe(new DispatchOrder(evn.CorrelationId, DateTime.Now)
         {
            PaymentStreamId = evn.StreamId,
            OrderStreamId = order.StreamId,
            Items = order.Items
         }.Execute(state));
      }
   }
}