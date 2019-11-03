using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Domain.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Infrastructure
{
   public class Persistence
   {
      private readonly HttpClient client = new HttpClient() {BaseAddress = new Uri("https://localhost:44344/") };
      private readonly JsonSerializerSettings settings;

      public Persistence()
      {
         this.settings = new JsonSerializerSettings
         {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
            , Converters = new List<JsonConverter> { new EventConverter() }
         };
      }

      public async Task Save<T>(IEnumerable<T> events)
      {
         foreach (var evn in events)
            await Save(evn);
      }

      public async Task Save<T>(T evn)
      {
         var json = JsonConvert.SerializeObject(evn, this.settings);
         var content = new StringContent(json, Encoding.UTF8, "application/json");
         var response = await this.client.PostAsync("stream", content);
      }

      private async Task<List<T>> LoadEventsAsync<T>(string url)
      {
         var response = await this.client.GetAsync(url);
         if (response.StatusCode != HttpStatusCode.OK) return new List<T>();
         var json = await response.Content.ReadAsStringAsync();
         var events = JsonConvert.DeserializeObject<List<T>>(json, this.settings);
         return events;
      }

      public Task<List<T>> Load<T>(string streamId)
         => LoadEventsAsync<T>($"stream/{streamId}");

      public Task<List<T>> Load<T>(string streamId, int fromVersion)
         => LoadEventsAsync<T>($"stream/{streamId}/{fromVersion}");

      public Task<List<T>> Load<T>(string streamId, int fromVersion, int toVersion) 
         => LoadEventsAsync<T>($"stream/{streamId}/{fromVersion}/{toVersion}");

      public async Task<T> LoadEvent<T>(string streamId, int version)
      {
         var events = await Load<T>(streamId, version, version);
         return events.FirstOrDefault();
      }

      public string CreateCategoryIndexStreamId(string category) => "byCategoryIndex-" + category;

      public async Task<T> GetState<T>(string streamId) where T : State
      {
         var zero = (T)Activator.CreateInstance(typeof(T), streamId);
         var events = await Load<Event>(streamId);
         events.ForEach(zero.Apply);
         return zero;
      }
   }
}
