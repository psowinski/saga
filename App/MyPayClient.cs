using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Common.Aggregate;
using Common.General;
using Domain.Model.Payment;

namespace App
{
   public class MyPayClient : IMyPayClient
   {
      private readonly HttpClient client = Https.CreateClient();

      public MyPayClient(string url)
      {
         this.client.BaseAddress = new Uri(url);
      }

      public async Task SendPaymentRequest(string requestId, decimal total, string description)
      {
         var json = $"{{\"requestId\": \"{requestId}\", " +
                    $"\"total\": {(int)(total*100)}, " +
                    $"\"description\": \"{description}\"}}";
         var content = new StringContent(json, Encoding.UTF8, "application/json");
         var response = await this.client.PostAsync("payment", content);
         if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception(response.ToString());
      }

      public async Task<PaymentStatus> CheckPayment(string requestId)
      {
         var response = await this.client.GetAsync($"payment/{requestId}/state");
         if (response.StatusCode == HttpStatusCode.NotFound)
            return PaymentStatus.Unpaid;
         if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception(response.ToString());

         var answer = await response.Content.ReadAsStringAsync();
         return answer switch
         {
            "pending" => PaymentStatus.Pending,
            "completed" => PaymentStatus.Completed,
            "cancelled" => PaymentStatus.Cancelled,
            _ => throw new Exception("Invalid response content: " + answer)
         };
      }
   }
}