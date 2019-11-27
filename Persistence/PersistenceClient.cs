using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Common.Aggregate;
using Common.General;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Persistence
{
   public class PersistenceClient : IPersistenceClient, IDisposable
   {
      private readonly HttpClient client = Https.CreateClient();
      private readonly JsonSerializerSettings settings;

      public PersistenceClient(string url)
      {
         this.client.BaseAddress = new Uri(url);
         this.settings = new JsonSerializerSettings
         {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = { new EventConverter() }
         };
      }

      private bool disposed = false;
      public void Dispose()
      {
         if (!disposed)
         {
            this.client.Dispose();
            disposed = true;
         }
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
         if (response.StatusCode != HttpStatusCode.OK) throw new Exception(response.ToString());
      }

      private async Task<List<T>> LoadEventsAsync<T>(string url)
      {
         var response = await this.client.GetAsync(url);
         if (response.StatusCode != HttpStatusCode.OK) throw new Exception(response.ToString());
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

      public async Task<TState> GetState<TState, TUpdater>(string streamId) 
         where TState : State where TUpdater : IEventUpdater<TState>, new()
      {
         var updater = new TUpdater();
         var zero = (TState)Activator.CreateInstance(typeof(TState), streamId);
         var events = await Load<AggregateEvent<TState>>(streamId);
         zero.Apply(events.SelectMany(updater.Update));
         return zero;
      }
   }
}
