using System;
using System.Threading.Tasks;
using Domain.Common;
using Domain.Delivery;
using Domain.Orders;
using Domain.Payment;
using Infrastructure;
using Sagas.Common;

namespace Sagas.Buying
{
   public class BuyingSagaAction : ISagaAction
   {
      private readonly Bus bus = new Bus();
      private readonly Persistence persistence = new Persistence();

      public async Task ProcessEvent(Event evn)
      {
         switch (evn)
         {
            case CheckedOutEvent checkedEvn:
               await OnCheckedEvent(checkedEvn);
               break;
            case PaidEvent paid:
               await OnPaidEvent(paid);
               break;
         }
      }

      private async Task OnCheckedEvent(CheckedOutEvent evn)
      {
         var order = await this.persistence.GetState<OrderState>(evn.StreamId);
         await PayOrder(order, evn);
      }

      private async Task OnPaidEvent(PaidEvent evn)
      {
         var order = await this.persistence.GetState<OrderState>(evn.OrderStreamId);
         await SendOrder(order, evn);
      }

      private async Task PayOrder(OrderState order, CheckedOutEvent evn)
      {
         var state = new PaymentState(NamesGenerator.GeneratePaymentId());

         await Delayer.WaitSomeTime();
         Console.WriteLine($"[{evn.CorrelationId}/{order.StreamId}] Payment [{state.StreamId}]");

         await this.bus.Pipe(new PayCommand(evn.CorrelationId, DateTime.Now)
         {
            OrderStreamId = order.StreamId,
            Amount = order.TotalCost
         }.Execute(state));
      }

      private async Task SendOrder(OrderState order, PaidEvent evn)
      {
         var state = new DeliveryState(NamesGenerator.GenerateDeliveryId());

         await Delayer.WaitSomeTime();
         Console.WriteLine($"[{evn.CorrelationId}/{order.StreamId}] Delivery [{state.StreamId}]");
         await this.bus.Pipe(new SendCommand(evn.CorrelationId, DateTime.Now)
         {
            PaymentStreamId = evn.StreamId,
            OrderStreamId = order.StreamId,
            Items = order.Items
          }.Execute(state));
      }
   }
}