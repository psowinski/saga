using System;
using System.Threading.Tasks;
using Domain;

namespace Runner
{
   public class BuyingSaga : ISaga
   {
      private readonly Bus bus = new Bus();
      private readonly Persistence persistence = new Persistence();

      public async Task ProcessEvent(Event evn)
      {
         switch (evn)
         {
            case CheckedEvent checkedEvn:
               await OnCheckedEvent(checkedEvn);
               break;
            case PaidEvent paid:
               await OnPaidEvent(paid);
               break;
         }
      }

      private async Task OnCheckedEvent(CheckedEvent evn)
      {
         var order = await this.persistence.GetState(evn.StreamId, new OrdersAggregate());
         await PayOrder(order, evn);
      }

      private async Task OnPaidEvent(PaidEvent evn)
      {
         var order = await this.persistence.GetState(evn.OrderStreamId, new OrdersAggregate());
         await SendOrder(order, evn);
      }

      private async Task PayOrder(OrderState order, CheckedEvent evn)
      {
         var payment = new PaymentAggregate();
         var state = payment.Zero(SagaUtils.GeneratePaymentId());

         await SagaUtils.WaitSomeTime();
         Console.WriteLine($"[{evn.CorrelationId}/{order.StreamId}] Payment [{state.StreamId}]");
         await this.bus.Pipe(payment.Execute(state, new PayCommand
         {
            CorrelationId = evn.CorrelationId,
            TimeStamp = SagaUtils.GenerateTimeStamp(),

            OrderStreamId = order.StreamId,
            Amount = order.TotalCost
         }));
      }

      private async Task SendOrder(OrderState order, PaidEvent evn)
      {
         var delivery = new DeliveryAggregate();
         var state = delivery.Zero(SagaUtils.GenerateDeliveryId());

         await SagaUtils.WaitSomeTime();
         Console.WriteLine($"[{evn.CorrelationId}/{order.StreamId}] Delivery [{state.StreamId}]");
         await this.bus.Pipe(delivery.Send(state, new SendCommand
         {
            CorrelationId = evn.CorrelationId,
            TimeStamp = SagaUtils.GenerateTimeStamp(),

            PaymentStreamId = evn.StreamId,
            OrderStreamId = order.StreamId,
            Items = order.Items
          }));
      }
   }
}