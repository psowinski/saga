using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System;
using Common.General;
using Saga;

namespace ShoppingSaga
{
   public class AppClient : IDisposable
   {
      private readonly HttpClient client = Https.CreateClient();

      public AppClient(string url)
      {
         this.client.BaseAddress = new Uri(url);
      }

      public Task<ActionStatus> Pay(string orderId, string correlationId)
         => PostAsync("payment", "{" +
               $"\"orderId\": \"{orderId}\", " +
               $"\"correlationId\": \"{correlationId}\"" +
            "}");

      public Task<ActionStatus> FinalizePayment(string paymentId, string correlationId, decimal total, string description)
         => PostAsync("payment/finalize", "{" +
                              $"\"paymentId\": \"{paymentId}\", " +
                              $"\"correlationId\": \"{correlationId}\", " +
                              $"\"total\": {total}, " +
                              $"\"description\": \"{description}\"" +
                              "}");


      public Task<ActionStatus> Dispatch(string orderId, string paymentId, string correlationId)
         => PostAsync("dispatch", "{" +
               $"\"orderId\": \"{orderId}\", " +
               $"\"paymentId\": \"{paymentId}\", " +
               $"\"correlationId\": \"{correlationId}\"" +
            "}");

      public async Task<ActionStatus> PostAsync(string address, string json)
      {
         var content = new StringContent(json, Encoding.UTF8, "application/json");
         var response = await this.client.PostAsync(address, content);

         return response.StatusCode switch
         {
            HttpStatusCode.OK => ActionStatus.Ok,
            HttpStatusCode.Accepted => ActionStatus.Pending,
            _ => throw new Exception(response.ToString())
         };
      }

      #region IDisposable Support
      private bool disposed = false;

      public void Dispose()
      {
         if (!disposed)
         {
            this.client.Dispose();
            disposed = true;
         }
      }
      #endregion
   }
}