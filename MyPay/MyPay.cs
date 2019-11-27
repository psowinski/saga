using Common.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyPay
{
   public class PaymentRequest
   {
      public string RequestId { get; set; }
      public uint Total { get; set; }
      public string Description { get; set; }
   }

   public enum PaymentStatus
   {
      Pending,
      Completed,
      Cancelled
   }

   public class Payment
   {
      public Payment()
      {
         Status = PaymentStatus.Pending;
      }

      public string RequestId { get; set; }
      public uint Total { get; set; }
      public string Description { get; set; }
      public PaymentStatus Status { get; private set; }

      public int TimeToConfirm { get; set; }

      public void Cancel() => Status = PaymentStatus.Cancelled;
      public void Complete() => Status = PaymentStatus.Completed;
   }

   public class MyPayService
   {
      private readonly object paymentLock = new object();
      private readonly List<Payment> payments = new List<Payment>();
      private readonly Random rnd = new Random();

      public Task RequestPayment(PaymentRequest request)
      {
         lock (paymentLock)
         {
            if (this.payments.All(x => x.RequestId != request.RequestId))
            {
               this.payments.Add(new Payment
               {
                  RequestId = request.RequestId,
                  Total = request.Total,
                  Description = request.Description,
                  TimeToConfirm = DateTime.Now.Millisecond // + rnd.Next(1000, 5000)
               }); ;
            }
            return Task.CompletedTask;
         }
      }

      public Task<Optional<PaymentStatus>> GetStatus(string requestId)
      {
         lock (paymentLock)
         {
            var payment = this.payments.FirstOrDefault(x => x.RequestId == requestId);
            if (payment != null)
            {
               if (payment.Status == PaymentStatus.Pending && payment.TimeToConfirm >= DateTime.Now.Millisecond)
               {
                  if (rnd.Next(0, 3) == 0) //reject 25%
                     payment.Cancel();
                  else
                     payment.Complete();

               }
               return Task.FromResult(new Some<PaymentStatus>(payment.Status) as Optional<PaymentStatus>);
            }
            return Task.FromResult(new None<PaymentStatus>() as Optional<PaymentStatus>);
         }
      }
   }
}
