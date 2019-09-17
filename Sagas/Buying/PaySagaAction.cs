using System;
using System.Threading.Tasks;
using Domain.Common;
using Domain.Orders;
using Domain.Payment;
using Infrastructure;
using Sagas.Common;

namespace Sagas.Buying
{
   public class PaySagaAction : ISagaAction
   {
      private readonly Bus bus = new Bus();
      private readonly Persistence persistence = new Persistence();

      public async Task ProcessEvent(Event evn)
      {
         if(evn is OrderCheckedOut checkedOut)
            await Pay(checkedOut);
         else
            throw new ArgumentException(nameof(evn));
      }

      private async Task Pay(OrderCheckedOut evn)
      {
         var state = new Payment(StreamNumbering.NewStreamId<Payment>());
         var order = await this.persistence.GetState<Order>(evn.StreamId);

         //await Delayer.WaitSomeTime();
         Console.WriteLine($"[{evn.CorrelationId}/{order.StreamId}] Payment [{state.StreamId}]");

         await this.bus.Pipe(new PayForOrder(evn.CorrelationId, DateTime.Now)
         {
            OrderStreamId = order.StreamId,
            Amount = order.TotalCost
         }.Execute(state));
      }
   }
}