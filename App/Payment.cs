using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Aggregate;
using Persistence;
using Common.General;
using Domain.BusinessLogic.Payment;
using Domain.Model.Payment;
using DomainOrder = Domain.Model.Order.Order;

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
         var state = new Domain.Model.Payment.V1.Payment(StreamNumbering.NewStreamId<Domain.Model.Payment.V1.Payment>());
         var order = await this.persistence.GetState<DomainOrder>(orderId);

         var payed = new Domain.BusinessLogic.Payment.V1.PayForOrder(correlationId, DateTime.Now)
         {
            OrderStreamId = order.StreamId,
            Amount = order.TotalCost
         }.Execute(state);

         await this.persistence.Save(payed);
      }

      public async Task Pay(string orderId, string correlationId)
      {
         var state = new Domain.Model.Payment.Payment(StreamNumbering.NewStreamId<Domain.Model.Payment.Payment>());
         var order = await this.persistence.GetState<DomainOrder>(orderId);

         var payed = new PayForOrder(correlationId, DateTime.Now)
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

         var state = await this.persistence.GetState<Domain.Model.Payment.Payment>(paymentId);
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
