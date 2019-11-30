using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Aggregate;
using Domain.Payment;
using Persistence;
using Common.General;
using DomainOrder = Domain.Order.Order;

namespace App
{
   public class Payment
   {
      private readonly IPersistenceClient persistence;
      private readonly IMyPayClient myPay;

      public Payment(IPersistenceClient persistence, IMyPayClient myPay)
      {
         this.persistence = persistence;
         this.myPay = myPay;
      }

      public async Task PayV1(string orderId, string correlationId)
      {
         var state = new PaymentV1(StreamNumbering.NewStreamId<PaymentV1>());
         var order = await this.persistence.GetState<DomainOrder>(orderId);

         var payed = new PayForOrderV1(correlationId, DateTime.Now)
         {
            OrderStreamId = order.StreamId,
            Amount = order.TotalCost
         }.Execute(state);

         await this.persistence.Save(payed);
      }

      public async Task PayV2(string orderId, string correlationId)
      {
         var state = new PaymentV2(StreamNumbering.NewStreamId<PaymentV2>());
         var order = await this.persistence.GetState<DomainOrder>(orderId);

         var payed = new PayForOrderV2(correlationId, DateTime.Now)
         {
            OrderStreamId = order.StreamId,
            Total = order.TotalCost,
            Description = order.Items.Aggregate((acc, x) => acc + ',' + x)
         }.Execute(state);

         await this.persistence.Save(payed);
         var _ = myPay.SendPaymentRequest(payed.StreamId, payed.Total, payed.Description).ConfigureAwait(false);
      }

      public async Task<bool> FinalizePayment(string paymentId, string correlationId, decimal total, string description)
      {
         var status = await this.myPay.CheckPayment(paymentId);
         if (status == PaymentStatus.Pending) return false;

         if (status == PaymentStatus.Unpaid)
         {
            await myPay.SendPaymentRequest(paymentId, total, description);
            return false;
         }

         var state = await this.persistence.GetState<PaymentV2>(paymentId);
         var evn = status switch
         {
            PaymentStatus.Completed => new CompletePayment(correlationId, DateTime.Now).Execute(state) as Event,
            PaymentStatus.Cancelled => new CancelPayment(correlationId, DateTime.Now).Execute(state),
            _ => throw new ArgumentException(nameof(status))
         };
         await this.persistence.Save(evn);
         return true;
      }
   }
}
