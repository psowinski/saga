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
         await PayOrder(order);
      }

      private async Task OnPaidEvent(PaidEvent evn)
      {
         var order = await this.persistence.GetState(evn.OrderStreamId, new OrdersAggregate());
         await SendOrder(evn, order);
      }

      private async Task PayOrder(OrderState order)
      {
         var payment = new PaymentAggregate();
         var state = payment.Zero(SagaUtils.GeneratePaymentId());

         await SagaUtils.DelayedHeader($"[{order.StreamId}] Payment [{state.StreamId}]");
         await this.bus.Pipe(payment.Pay(state, order.StreamId, order.TotalCost));
      }

      private async Task SendOrder(PaidEvent paid, OrderState order)
      {
         var delivery = new DeliveryAggregate();
         var state = delivery.Zero(SagaUtils.GenerateDeliveryId());

         await SagaUtils.DelayedHeader($"[{order.StreamId}] Delivery [{state.StreamId}]");
         await this.bus.Pipe(delivery.Send(state, paid.StreamId, order.StreamId, order.Items));
      }
   }
}