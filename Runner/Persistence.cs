using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Newtonsoft.Json;
using Infrastructure;

namespace Runner
{
   public class Persistence
   {
      private static readonly Database Db = new Database();
      private readonly JsonSerializerSettings settings;

      public static string Dump() => Db.Dump();

      public Persistence()
      {
         this.settings = new JsonSerializerSettings
         {
            TypeNameHandling = TypeNameHandling.Objects,
            SerializationBinder = new DisplayNameSerializationBinder()
         };
      }

      public async Task Save(IEnumerable<Event> events)
      {
         foreach (var evn in events)
            await Save(evn);
      }

      public Task Save(Event evn) => Db.Save(JsonConvert.SerializeObject(evn, this.settings));

      public Task<List<Event>> Load(string streamId) => Load(streamId, 0);

      public Task<List<Event>> Load(string streamId, int fromVersion) => Load(streamId, fromVersion, 0);

      public async Task<List<Event>> Load(string streamId, int fromVersion, int toVersion)
      {
         var json = await Db.Load(streamId, fromVersion, toVersion);
         var events = JsonConvert.DeserializeObject<List<Event>>(json, this.settings);
         return events;
      }

      public async Task<Event> LoadEvent(string streamId, int version)
      {
         var events = await Load(streamId, version, version);
         return events.FirstOrDefault();
      }

      public string GetCategoryStreamId(string category) => "ByCategoryIndex-" + category;

      public async Task<int> GetLastStreamVersion(string streamId) => await Db.FindLastVersion(streamId);

      public async Task<T> GetState<T>(string streamId, IAggregate<T> aggregate)
      {
         var zero = aggregate.Zero(streamId);
         var events = await Load(streamId);

         return events.Aggregate(zero, aggregate.Apply);
      }
   }
}
