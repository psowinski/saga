using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Persistence
{
   public class Database
   {
      private readonly List<dynamic> data = new List<dynamic>();
      public void Save(string json)
      {
         var obj = JsonConvert.DeserializeObject<dynamic>(json);
         this.data.Add(obj);
      }

      public string Load(string id)
      {
         var events = this.data.Where(x => x.Id == id);
         var json = JsonConvert.SerializeObject(events);
         return json;
      }
   }
}
