using System;
using Orders;

namespace Runner
{
   public class BuyingSaga
   {
      private readonly Bus bus;
      public BuyingSaga(Bus bus)
      {
         this.bus = bus;
      }

      public void OnEvent(Event evn)
      {
         switch (evn)
         {
            case CheckedEvent checkedEvn:
               OnCheckedEvent(checkedEvn);
               break;
            case PaidEvent paid:
               OnPaidEvent(paid);
               break;
         }
      }

      private void OnCheckedEvent(CheckedEvent evn)
      {
         Console.WriteLine("Read order");
         var order = ReadOrder(evn.StreamId);

         Console.WriteLine("Pay");
         PayOrder(order);
      }

      private void OnPaidEvent(PaidEvent evn)
      {
         Console.WriteLine("Read order");
         var order = ReadOrder(evn.StreamId);

         Console.WriteLine("Send");
         SendOrder(evn, order);
      }

      private void PayOrder(OrderState order)
      {
         var payment = new PaymentAggregate();
         var state = payment.Zero("payment-1");
         this.bus.Pipe(payment.Pay(state, order.StreamId, order.TotalCost));
      }

      private void SendOrder(PaidEvent paid, OrderState order)
      {
         var delivery = new DeliveryAggregate();
         var state = delivery.Zero("delivery-1");
         this.bus.Pipe(delivery.Send(state, paid.StreamId, order.StreamId, order.Items));
      }

      private OrderState ReadOrder(string streamId)
      {
         var orders = new OrdersAggregate();
         var state = orders.Zero(streamId);
         var events = this.bus.Load(streamId);
         foreach (var evn in events)
            state = orders.Apply(state, evn);
         return state;
      }
   }
}