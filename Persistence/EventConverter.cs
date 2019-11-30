using System;
using System.Collections.Generic;
using System.Linq;
using Common.Aggregate;
using Domain.Model.Order;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Persistence
{
   public class EventConverter : JsonConverter
   {
      private readonly JsonSerializer camelCase = new JsonSerializer()
      {
         ContractResolver = new CamelCasePropertyNamesContractResolver()
      };

      private readonly Dictionary<string, Type> nameToType;

      public EventConverter()
      {
         this.nameToType = typeof(Event).Assembly.GetTypes()
            .Union(typeof(Order).Assembly.GetTypes())
            .Where(x => typeof(Event).IsAssignableFrom(x) && x != typeof(Event) && x != typeof(AggregateEvent<>))
            .ToDictionary(t => t.Name, t => t);
      }

      public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
      {
         var token = JObject.FromObject(value, this.camelCase);
         token.AddFirst(new JProperty("type", value.GetType().Name));
         token.WriteTo(writer);
      }

      public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
      {
         var jObject = JObject.Load(reader);

         var typeName = jObject["type"].Value<string>();
         var target = Activator.CreateInstance(this.nameToType[typeName]);

         var objectReader = CreateObjectReader(reader, jObject);
         serializer.Populate(objectReader, target);

         return target;
      }

      public override bool CanConvert(Type objectType)
         => objectType == typeof(Event) || objectType.IsSubclassOf(typeof(Event));

      public JsonReader CreateObjectReader(JsonReader reader, JToken jToken)
      {
         var readerCopy = jToken.CreateReader();
         readerCopy.Culture = reader.Culture;
         readerCopy.DateFormatString = reader.DateFormatString;
         readerCopy.DateParseHandling = reader.DateParseHandling;
         readerCopy.DateTimeZoneHandling = reader.DateTimeZoneHandling;
         readerCopy.FloatParseHandling = reader.FloatParseHandling;
         readerCopy.MaxDepth = reader.MaxDepth;
         readerCopy.SupportMultipleContent = reader.SupportMultipleContent;
         return readerCopy;
      }
   }
}
