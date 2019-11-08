using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System;
using Common.General;

namespace UserScenarioApp
{
   public class AppClient : IDisposable
   {
      private readonly HttpClient client = Https.CreateClient();

      public AppClient(string url)
      {
         this.client.BaseAddress = new Uri(url);
      }

      public Task AddItem(string orderId, string correlationId, string description, decimal cost)
         => PostAsync("order/additem", "{" +
               $"\"orderId\": \"{orderId}\", " +
               $"\"correlationId\": \"{correlationId}\"," +
               $"\"description\": \"{description}\", " +
               $"\"cost\": {cost}" +
            "}");

      public Task Checkout(string orderId, string correlationId)
         => PostAsync("order/checkout", "{" +
               $"\"orderId\": \"{orderId}\", " +
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