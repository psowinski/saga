using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System;

namespace ShoppingSaga
{
   public class AppClient : IDisposable
   {
      private readonly HttpClient client = new HttpClient();

      public AppClient(string url)
      {
         this.client.BaseAddress = new Uri(url);
      }

      public Task Pay(string orderId, string correlationId)
         => PostAsync("payment", "{" +
               $"\"orderId\": \"{orderId}\", " +
               $"\"correlationId\": \"{correlationId}\"" +
            "}");

      public Task Dispatch(string orderId, string paymentId, string correlationId)
         => PostAsync("dispatch", "{" +
               $"\"orderId\": \"{orderId}\", " +
               $"\"paymentId\": \"{paymentId}\", " +
               $"\"correlationId\": \"{correlationId}\"" +
            "}");

      public async Task PostAsync(string address, string json)
      {
         var content = new StringContent(json, Encoding.UTF8, "application/json");
         var response = await this.client.PostAsync(address, content);
         if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception(response.ToString());
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