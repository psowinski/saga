using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Orders;
using Persistence;

namespace Runner
{
   public class Persistence
   {
      private static readonly Database Db = new Database();
      private readonly JsonSerializerSettings settings;

      public Persistence()
      {
         this.settings = new JsonSerializerSettings
         {
            TypeNameHandling = TypeNameHandling.Objects,
            SerializationBinder = new DisplayNameSerializationBinder()
         };
      }

      public Task Save(IEnumerable<Event> events)
      {
         return Task.WhenAll(events.Select(evn => Db.Save(JsonConvert.SerializeObject(evn, this.settings))).ToList());
      }

      public async Task<List<Event>> Load(string streamId)
      {
         var json = await Db.Load(streamId);
         var events = JsonConvert.DeserializeObject<List<Event>>(json, this.settings);
         return events;
      }

      public async Task<OrderState> GetOrder(string streamId)
      {
         var orders = new OrdersAggregate();

         var zero = orders.Zero(streamId);
         var events = await Load(streamId);

         return events.Aggregate(zero, (acc, evn) => orders.Apply(acc, evn));
      }
   }
}
