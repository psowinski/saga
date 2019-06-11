using System;
using System.Linq;
using System.Threading.Tasks;
using Orders;

namespace Runner
{
   public class BuyingSaga
   {
      private readonly Bus bus;
      private readonly Persistence persistence;
      public BuyingSaga(Bus bus, Persistence persistence)
      {
         this.bus = bus;
         this.persistence = persistence;
      }

      public async Task OnEvent(Event evn)
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
         var order = await this.persistence.GetOrder(evn.StreamId);
         await PayOrder(order);
      }

      private async Task OnPaidEvent(PaidEvent evn)
      {
         var order = await this.persistence.GetOrder(evn.OrderStreamId);
         await SendOrder(evn, order);
      }

      private readonly Random rnd = new Random();

      private async Task PayOrder(OrderState order)
      {
         var payment = new PaymentAggregate();
         var state = payment.Zero(Saga.GeneratePaymentId());

         await Saga.DelayedHeader($"[{order.StreamId}] Payment [{state.StreamId}]");
         await this.bus.Pipe(payment.Pay(state, order.StreamId, order.TotalCost));
      }

      private async Task SendOrder(PaidEvent paid, OrderState order)
      {
         var delivery = new DeliveryAggregate();
         var state = delivery.Zero(Saga.GenerateDeliveryId());

         await Saga.DelayedHeader($"[{order.StreamId}] Delivery [{state.StreamId}]");
         await this.bus.Pipe(delivery.Send(state, paid.StreamId, order.StreamId, order.Items));
      }
   }
}