using System.Threading.Tasks;
using Domain.Payment;

namespace App
{
   public interface IMyPayClient
   {
      Task SendPaymentRequest(string requestId, decimal total, string description);
      Task<PaymentStatus> CheckPayment(string requestId);
   }
}