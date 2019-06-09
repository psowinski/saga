using System.Collections.Generic;
using Newtonsoft.Json;
using Orders;
using Persistence;

namespace Runner
{
   public class Bus
   {
      private readonly Database db = new Database();
      private readonly BuyingSaga buyingSaga;

      public Bus()
      {
         buyingSaga = new BuyingSaga(this);
      }

      public void Pipe(Event evn)
      {
         var inArray = new[] {evn};
         Save(inArray);
         Process(inArray);
      }

      public void Pipe(IEnumerable<Event> events)
      {
         Save(events);
         Process(events);
      }

      private void Process(IEnumerable<Event> events)
      {
         foreach (var evn in events)
            OnEvent(evn);
      }

      public void OnEvent(Event evn)
      {
         this.buyingSaga.OnEvent(evn);
      }

      private void Save(IEnumerable<Event> events)
      {
         var settings = new JsonSerializerSettings { SerializationBinder = new DisplayNameSerializationBinder() };
         foreach (var evn in events)
            db.Save(JsonConvert.SerializeObject(evn, settings));
      }
      public List<Event> Load(string streamId)
      {
         var json = this.db.Load(streamId);
         var settings = new JsonSerializerSettings { SerializationBinder = new DisplayNameSerializationBinder() };
         var events = JsonConvert.DeserializeObject<List<Event>>(json, settings);
         return events;
      }
   }
}
