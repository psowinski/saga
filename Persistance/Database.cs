using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Persistence
{
   public class Database
   {
      private readonly List<dynamic> data = new List<dynamic>();
      public Task Save(string json)
      {
         json = json.Replace("$type", "EventType");
         lock (data)
         {
            var obj = JsonConvert.DeserializeObject<dynamic>(json);
            this.data.Add(obj);
            return Task.CompletedTask;
         }
      }

      public Task<string> Load(string id)
      {
         lock (data)
         {
            var events = this.data.Where(x => x.StreamId == id);
            var json = JsonConvert.SerializeObject(events);
            json = json.Replace("EventType", "$type");
            return Task.FromResult(json);
         }
      }
   }
}
